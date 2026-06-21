#!/usr/bin/env python3
# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

"""Regenerates the CWE taxonomy artifacts under src/Sarif/Taxonomies/ from
the authoritative MITRE Common Weakness Enumeration (CWE) XML catalog.

Downloads cwec_latest.xml.zip from cwe.mitre.org, extracts the XML,
parses the entries, and emits three consolidated artifacts covering all
maturity statuses (Stable, Draft, Incomplete, Deprecated, Obsolete):

    CweTaxonomy.sarif       SARIF 2.1.0 taxonomy with verbatim MITRE content.
                            Each taxon carries fullDescription (Description +
                            Extended Description) and help content (Description
                            + Extended Description + Common Consequences +
                            Potential Mitigations) in both help.text and
                            help.markdown. shortDescription is emitted only when
                            a consumer cannot recover it from the first sentence
                            of fullDescription (SARIF §3.49.2, §3.49.10), keeping
                            the file as small as possible.

    CweTaxonomy.brief.md    Compact markdown table sized for AI prompt
                            context-window injection.

    CweCategories.json      Compact id -> name map of every MITRE CWE Category.
                            A Category is an organizational grouping, never a
                            valid result.ruleId mapping target; consumers use
                            this to recognize and explain that class of
                            mis-mapping.

Both files are written with UTF-8 (no BOM) and LF line endings regardless of
the host OS so checked-in artifacts stay byte-stable across contributors.

CWE is published by MITRE Corporation under terms permitting redistribution
with attribution. The generated taxonomies preserve attribution metadata.

Usage (zero args — downloads, extracts, regenerates in place)::

    python3 scripts/generate_cwe_taxonomy.py

Or against a pre-extracted XML (useful for offline / testing)::

    python3 scripts/generate_cwe_taxonomy.py --xml path/to/cwec_v4.20.xml
"""

import argparse
import io
import json
import re
import sys
import tempfile
import urllib.request
import zipfile
from pathlib import Path
from xml.etree import ElementTree as ET

NS = {"c": "http://cwe.mitre.org/cwe-7"}
ALL_STATUSES = ["Stable", "Draft", "Incomplete", "Deprecated", "Obsolete"]
SARIF_SCHEMA = "https://schemastore.azurewebsites.net/schemas/json/sarif-2.1.0.json"
DEFAULT_SOURCE_URL = "https://cwe.mitre.org/data/xml/cwec_latest.xml.zip"


def repo_root():
    return Path(__file__).resolve().parent.parent


def default_output_dir():
    return repo_root() / "src" / "Sarif" / "Taxonomies"


def load_security_severity():
    """Load the SDK's curated per-CWE security-severity table
    (src/Sarif/Taxonomies/CweSecuritySeverity.json), keyed by CWE id (e.g. "CWE-89").
    This is the SDK's hand-curated severity prior, not MITRE data; the taxonomy carries
    it so it is a formal part of the CWE story and flows to CWE-as-rule descriptors."""
    path = repo_root() / "src" / "Sarif" / "Taxonomies" / "CweSecuritySeverity.json"
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)


def download_and_extract(source_url, staging_dir):
    """Download ``source_url`` and extract the embedded CWE XML into
    ``staging_dir``. Returns the path to the extracted ``cwec_v*.xml``."""
    staging_dir = Path(staging_dir)
    staging_dir.mkdir(parents=True, exist_ok=True)

    zip_path = staging_dir / "cwec_latest.xml.zip"
    print(f"[1/4] Downloading {source_url} ...")
    with urllib.request.urlopen(source_url) as response, open(zip_path, "wb") as out:
        out.write(response.read())

    print(f"[2/4] Extracting {zip_path.name} ...")
    with zipfile.ZipFile(zip_path) as zf:
        zf.extractall(staging_dir)

    xml_candidates = sorted(staging_dir.glob("cwec_v*.xml"))
    if not xml_candidates:
        raise FileNotFoundError(f"No cwec_v*.xml found in {staging_dir} after extraction.")
    return xml_candidates[0]


def clean_text(node):
    """Return whitespace-normalized text content for ``node``.

    CWE XML descriptions are mixed content (text + nested xhtml elements).
    The PowerShell counterpart uses ``$node.InnerText`` which flattens all
    descendant text; the Python equivalent walks ``itertext()`` and joins.
    Whitespace runs collapse to a single space.
    """
    if node is None:
        return ""
    text = "".join(node.itertext())
    return re.sub(r"\s+", " ", text).strip()


_PAREN_NAME_RE = re.compile(r"\('([^']+)'\)|\(\"([^\"]+)\"\)")
_NON_ALNUM_RE = re.compile(r"[^A-Za-z0-9]+")

# A sentence terminator is . ! or ? plus any closing quotes/brackets,
# followed by whitespace or end-of-string. Requiring trailing whitespace
# means the internal dots of abbreviations like "e.g." (followed by a
# letter, not a space) are never candidate boundaries.
_SENTENCE_END_RE = re.compile(r"[.!?][\"')\]]*(?:\s|$)")

# Lowercased tokens (trailing run up to and including the period) that look
# like a sentence end but are not. Used to skip false boundaries.
_ABBREVIATIONS = frozenset([
    "e.g.", "i.e.", "etc.", "vs.", "cf.", "resp.", "al.", "approx.",
    "inc.", "corp.", "ltd.", "co.", "no.", "fig.", "ref.", "a.k.a.",
])


def first_sentence(text):
    """Return the first sentence of ``text`` verbatim.

    The result is always a literal prefix of ``text`` (or the whole string
    when no boundary is found) — never a paraphrase — so a shortDescription
    derived from a MITRE Description stays definitively MITRE-sourced.

    Boundary detection skips common abbreviations (``e.g.``, ``i.e.``,
    ``etc.``), single-letter initials (``U.S.``), terminators sitting inside
    an unclosed parenthetical (the dot in ``(e.g.`` or the tag
    ``[PLANNED FOR DEPRECATION.``), and terminators followed by a lowercase
    continuation (a quoted example like ``".."`` that the sentence resumes
    after). When in doubt it errs toward *not* splitting, so the worst case
    is the unchanged full text.
    """
    if not text:
        return text
    for m in _SENTENCE_END_RE.finditer(text):
        punct_idx = m.start()
        token_match = re.search(r"\S+$", text[:punct_idx + 1])
        token = token_match.group(0).lower() if token_match else ""
        # Drop leading openers so "(e.g." is recognized as the abbreviation
        # "e.g." rather than read as a distinct, unguarded token.
        token = token.lstrip("([{\"'")
        if token in _ABBREVIATIONS:
            continue
        # Single-letter initial at a word boundary, e.g. the "U." / "S." in
        # "U.S.": an uppercase letter preceded by a non-alphanumeric char.
        if text[punct_idx - 1:punct_idx].isupper():
            before = text[punct_idx - 2:punct_idx - 1]
            if before == "" or not before.isalnum():
                continue
        candidate = text[:m.end()]
        # A terminator inside an unclosed parenthetical or bracket is not a
        # real sentence end.
        if (candidate.count("(") != candidate.count(")")
                or candidate.count("[") != candidate.count("]")
                or candidate.count("{") != candidate.count("}")):
            continue
        # A real boundary is followed by end-of-text or the start of a new
        # sentence (an uppercase letter or a digit). A lowercase next
        # character means the terminator is internal -- a quoted example or
        # an abbreviation -- and the sentence actually continues.
        rest = text[m.end():].lstrip()
        if rest and not (rest[0].isupper() or rest[0].isdigit()):
            continue
        return candidate.strip()
    return text.strip()


def naive_first_sentence(text):
    """Return the first sentence using only the bare terminator rule.

    This is the dead-simple extraction a downstream consumer applies to
    recover a shortDescription from a fullDescription: the prefix up to and
    including the first ``.``/``!``/``?`` (with any trailing closing quotes or
    brackets) that is followed by whitespace or end-of-text. Unlike
    :func:`first_sentence` it has no abbreviation, parenthetical, or
    lowercase-continuation guards.

    The generator omits shortDescription only when this naive result equals the
    curated :func:`first_sentence`, so a consumer using this rule reconstructs
    the curated value exactly. Where the two disagree (a guarded boundary), the
    curated shortDescription is retained verbatim.
    """
    if not text:
        return text
    m = _SENTENCE_END_RE.search(text)
    if m:
        return text[:m.end()].strip()
    return text.strip()


def pascal_case_name(cwe_name):
    """Derive a single Pascal-case identifier from a CWE Name that satisfies
    the SARIF2012 strict Pascal-case regex ``^(\\p{Lu}[\\p{Ll}\\p{Nd}]+)*$``.

    Strategy mirrors ``Get-PascalCaseName`` in the PowerShell script:

    1. If the CWE Name contains a parenthesized quoted short name like
       ``('Cross-site Scripting')``, prefer that.
    2. Otherwise fall back to the full CWE Name.

    Either way, split on every non-alphanumeric character and title-case
    each fragment (first char upper, rest lower) before concatenating. This
    correctly produces ``Sql`` from ``SQL`` / ``J2ee`` from ``J2EE``, as
    required by the validator's strict regex.
    """
    if not cwe_name or not cwe_name.strip():
        return None

    source = cwe_name
    paren = _PAREN_NAME_RE.search(cwe_name)
    if paren:
        source = paren.group(1) if paren.group(1) is not None else paren.group(2)

    fragments = [f for f in _NON_ALNUM_RE.split(source) if f]
    if not fragments:
        return None

    parts = []
    for f in fragments:
        if len(f) == 1:
            parts.append(f.upper())
        else:
            parts.append(f[0].upper() + f[1:].lower())
    return "".join(parts)


def view1000_parent(weakness):
    """Return ``CWE-<id>`` of the canonical ChildOf parent under CWE View 1000.

    When multiple ChildOf relations exist on the same view, prefer the one
    marked ``Ordinal=Primary``; otherwise pick the first.
    """
    rels = weakness.find("c:Related_Weaknesses", NS)
    if rels is None:
        return None
    candidates = [
        r for r in rels.findall("c:Related_Weakness", NS)
        if r.get("Nature") == "ChildOf" and r.get("View_ID") == "1000"
    ]
    if not candidates:
        return None
    primary = next((r for r in candidates if r.get("Ordinal") == "Primary"), None)
    parent = primary if primary is not None else candidates[0]
    return f"CWE-{parent.get('CWE_ID')}"


def build_help(weakness, markdown=True):
    """Produce the verbatim help block embedded on each taxon.

    With ``markdown=True`` the result is the rich ``help.markdown`` value; with
    ``markdown=False`` it is the plaintext twin stored in ``help.text`` — the
    same sections and structure with the markdown markup (``##`` headers,
    ``**bold**``, ``*italic*``) removed. The two are faithful renderings of one
    body of content, per the SARIF multiformatMessageString contract.

    Sections (in order, each only present when source content is non-empty):

    - ``Description`` (CWE Description)
    - ``Extended Description`` (CWE Extended_Description)
    - ``Common Consequences`` (CWE Common_Consequences/Consequence)
    - ``Potential Mitigations`` (CWE Potential_Mitigations/Mitigation)
    """
    head = "## " if markdown else ""
    strong = "**" if markdown else ""
    emph = "*" if markdown else ""
    out = io.StringIO()

    desc = clean_text(weakness.find("c:Description", NS))
    if desc:
        out.write(f"{head}Description\n\n")
        out.write(desc)
        out.write("\n\n")

    ext = clean_text(weakness.find("c:Extended_Description", NS))
    if ext:
        out.write(f"{head}Extended Description\n\n")
        out.write(ext)
        out.write("\n\n")

    cc = weakness.find("c:Common_Consequences", NS)
    if cc is not None:
        consequences = cc.findall("c:Consequence", NS)
        if consequences:
            out.write(f"{head}Common Consequences\n\n")
            for c in consequences:
                scopes = [clean_text(s) for s in c.findall("c:Scope", NS)]
                impacts = [clean_text(i) for i in c.findall("c:Impact", NS)]
                scope_text = ", ".join(s for s in scopes if s)
                impact_text = ", ".join(i for i in impacts if i)
                line = "- "
                if scope_text:
                    line += f"{strong}Scope{strong}: {scope_text}. "
                if impact_text:
                    line += f"{strong}Impact{strong}: {impact_text}."
                out.write(line.rstrip() + "\n")
                note = clean_text(c.find("c:Note", NS))
                if note:
                    out.write(f"  - {note}\n")
            out.write("\n")

    pm = weakness.find("c:Potential_Mitigations", NS)
    if pm is not None:
        mitigations = pm.findall("c:Mitigation", NS)
        if mitigations:
            out.write(f"{head}Potential Mitigations\n\n")
            for i, m in enumerate(mitigations, start=1):
                phases = [clean_text(p) for p in m.findall("c:Phase", NS)]
                phase_text = ", ".join(p for p in phases if p)
                strategy_text = clean_text(m.find("c:Strategy", NS))
                tag_parts = []
                if phase_text:
                    tag_parts.append(f"Phase: {phase_text}")
                if strategy_text:
                    tag_parts.append(f"Strategy: {strategy_text}")
                tag = emph + "; ".join(tag_parts) + emph if tag_parts else ""
                desc_text = clean_text(m.find("c:Description", NS))
                line = f"{i}. "
                if tag:
                    line += f"{tag}. "
                line += desc_text
                out.write(line + "\n")
            out.write("\n")

    return out.getvalue().rstrip()


def parse_cwe_version(xml_path):
    """Best-effort extract the catalog version (e.g. ``4.20``) from the file name."""
    m = re.search(r"cwec_v([\d.]+)\.xml", Path(xml_path).name)
    return m.group(1) if m else "0.0"


def emit(xml_path, output_dir, source_url):
    output_dir = Path(output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)

    cwe_version = parse_cwe_version(xml_path)
    tree = ET.parse(xml_path)
    root = tree.getroot()

    print(f"[3/4] Parsing {Path(xml_path).name} (CWE v{cwe_version}) ...")
    bucketed = {s: [] for s in ALL_STATUSES}
    weaknesses = root.findall(".//c:Weaknesses/c:Weakness", NS)
    skipped = 0
    for w in weaknesses:
        status = w.get("Status")
        if status not in bucketed:
            skipped += 1
            continue
        bucketed[status].append(w)
    print(f"  Parsed {len(weaknesses)} weaknesses; skipped {skipped} (unrecognized status).")
    for s in ALL_STATUSES:
        print(f"    {s:<11} {len(bucketed[s]):>4} entries")

    # Categories are organizational groupings, never valid result.ruleId mapping
    # targets. We capture their id -> name (across every status) so a consumer can
    # recognize the single most common AI mis-mapping -- naming a Category instead
    # of the specific Weakness under it -- and say so in its own output.
    categories = root.findall(".//c:Categories/c:Category", NS)
    category_map = {f"CWE-{c.get('ID')}": c.get("Name") for c in categories}
    category_map = dict(sorted(category_map.items(), key=lambda kv: int(kv[0].split("-", 1)[1])))
    print(f"  Parsed {len(category_map)} categories.")

    print(f"[4/4] Writing consolidated artifacts to {output_dir} ...")
    all_items = sorted(
        (w for s in ALL_STATUSES for w in bucketed[s]),
        key=lambda w: int(w.get("ID"))
    )

    taxa = []
    security_severity = load_security_severity()
    for w in all_items:
        cwe_id = f"CWE-{w.get('ID')}"
        title = w.get("Name")
        name = pascal_case_name(title)
        desc_text = clean_text(w.find("c:Description", NS))
        curated_short = first_sentence(desc_text)

        ext = clean_text(w.find("c:Extended_Description", NS))
        full_text = desc_text + ("\n\n" + ext if ext else "")

        taxon = {
            "id": cwe_id,
            "name": name,
        }

        # fullDescription begins with the Description, whose first sentence is
        # the curated short (SARIF §3.49.10). Emit shortDescription only when a
        # consumer's bare first-sentence rule would NOT recover the curated
        # value; otherwise omit it and let the consumer derive it from
        # fullDescription (SARIF §3.49.2 — full alone satisfies the constraint).
        if curated_short and naive_first_sentence(full_text) != curated_short:
            taxon["shortDescription"] = {"text": curated_short}

        taxon["fullDescription"] = {"text": full_text}

        help_md = build_help(w, markdown=True)
        if help_md:
            taxon["help"] = {
                "text": build_help(w, markdown=False),
                "markdown": help_md,
            }
        taxon["helpUri"] = f"https://cwe.mitre.org/data/definitions/{w.get('ID')}.html"

        props = {
            "cwe/title": title,
            "cwe/status": w.get("Status"),
            "cwe/abstraction": w.get("Abstraction", ""),
        }
        parent = view1000_parent(w)
        if parent:
            props["cwe/parent"] = parent
        sev = security_severity.get(cwe_id)
        if sev is not None:
            props["security-severity"] = f"{sev:.1f}"
        taxon["properties"] = props

        taxa.append(taxon)

    status_counts = ", ".join(f"{s}={len(bucketed[s])}" for s in ALL_STATUSES)
    taxonomy = {
        "name": "CWE",
        "version": cwe_version,
        "organization": "MITRE",
        "informationUri": "https://cwe.mitre.org/",
        "downloadUri": source_url,
        "isComprehensive": True,
        "minimumRequiredLocalizedDataSemanticVersion": "1.0.0",
        "shortDescription": {
            "text": f"MITRE Common Weakness Enumeration (CWE). {len(all_items)} entries from cwec_v{cwe_version}.xml."
        },
        "fullDescription": {
            "text": (
                "Complete snapshot of the MITRE CWE catalog. Each taxon carries "
                f"cwe/status ({status_counts}), cwe/abstraction, (where applicable) "
                "cwe/parent (canonical ChildOf parent under CWE View 1000 / Research Concepts), "
                "and (where curated) security-severity (the SDK's stable per-CWE severity prior "
                "on the 0.0-10.0 scale read by GitHub and Azure DevOps Advanced Security)."
            )
        },
        "taxa": taxa,
    }

    run = {
        "tool": {
            "driver": {
                "name": "Microsoft.CodeAnalysis.Sarif.Taxonomies.CweGenerator",
                "informationUri": "https://github.com/microsoft/sarif-sdk",
            }
        },
        "taxonomies": [taxonomy],
    }

    sarif_log = {
        "$schema": SARIF_SCHEMA,
        "version": "2.1.0",
        "runs": [run],
    }

    sarif_path = output_dir / "CweTaxonomy.sarif"
    sarif_text = json.dumps(sarif_log, indent=2, ensure_ascii=False)
    with open(sarif_path, "w", encoding="utf-8", newline="\n") as f:
        f.write(sarif_text)

    # ---- Brief markdown table ----
    md = io.StringIO()
    md.write("# CWE\n\n")
    md.write(
        f"Compact one-row-per-entry index of the MITRE CWE catalog (cwec_v{cwe_version}.xml). "
        f"{len(all_items)} entries ("
        f"Stable {len(bucketed['Stable'])}, "
        f"Draft {len(bucketed['Draft'])}, "
        f"Incomplete {len(bucketed['Incomplete'])}, "
        f"Deprecated {len(bucketed['Deprecated'])}, "
        f"Obsolete {len(bucketed['Obsolete'])}). "
        "Sorted by numeric ID.\n\n"
    )
    md.write(
        "Designed for AI prompt-context injection: full id form (`CWE-NNNN`), name, "
        "abstraction level (Pillar/Class/Base/Variant/Compound), MITRE maturity "
        "status, parent in View 1000 (Research Concepts), and the verbatim MITRE "
        "Description.\n\n"
    )
    md.write("| ID | Name | Abstraction | Status | Parent | Description |\n")
    md.write("|----|------|-------------|--------|--------|-------------|\n")

    for w in all_items:
        id_cell = f"CWE-{w.get('ID')}"
        name_cell = pascal_case_name(w.get("Name")) or ""
        abs_cell = w.get("Abstraction", "")
        status_cell = w.get("Status")
        parent = view1000_parent(w)
        parent_cell = parent if parent else ""
        desc_src = clean_text(w.find("c:Description", NS))
        desc_cell = desc_src.replace("|", "\\|")
        md.write(f"| {id_cell} | {name_cell} | {abs_cell} | {status_cell} | {parent_cell} | {desc_cell} |\n")

    brief_path = output_dir / "CweTaxonomy.brief.md"
    with open(brief_path, "w", encoding="utf-8", newline="\n") as f:
        f.write(md.getvalue())

    # ---- Categories map ----
    categories_doc = {
        "version": cwe_version,
        "comment": (
            f"MITRE CWE Categories (id -> name) from cwec_v{cwe_version}.xml, generated "
            "from the same catalog as CweTaxonomy.sarif. A Category is an organizational "
            "grouping, never a valid result.ruleId mapping target; consumers use this to "
            "recognize and explain that class of mis-mapping."
        ),
        "categories": category_map,
    }
    categories_path = output_dir / "CweCategories.json"
    categories_text = json.dumps(categories_doc, indent=2, ensure_ascii=False)
    with open(categories_path, "w", encoding="utf-8", newline="\n") as f:
        f.write(categories_text)

    print(
        f"  -> CweTaxonomy.sarif ({sarif_path.stat().st_size:,} bytes) / "
        f"CweTaxonomy.brief.md ({brief_path.stat().st_size:,} bytes) / "
        f"CweCategories.json ({categories_path.stat().st_size:,} bytes) -- "
        f"{len(all_items):,} taxa, {len(category_map):,} categories"
    )
    print("Done.")


def main():
    parser = argparse.ArgumentParser(
        description=__doc__,
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    parser.add_argument(
        "--xml",
        help=(
            "Path to a pre-extracted MITRE CWE XML catalog (e.g. cwec_v4.20.xml). "
            "If omitted, the catalog is downloaded from --source-url and extracted "
            "to a temporary staging directory."
        ),
    )
    parser.add_argument(
        "--output-dir",
        default=str(default_output_dir()),
        help="Output directory for CweTaxonomy.sarif and CweTaxonomy.brief.md "
             "(default: src/Sarif/Taxonomies/ at the repository root).",
    )
    parser.add_argument(
        "--source-url",
        default=DEFAULT_SOURCE_URL,
        help=f"Source URL for download and for recording in taxonomy.downloadUri "
             f"(default: {DEFAULT_SOURCE_URL}).",
    )
    args = parser.parse_args()

    if args.xml:
        emit(args.xml, args.output_dir, args.source_url)
        return

    with tempfile.TemporaryDirectory(prefix="cwe-taxonomy-") as staging:
        xml_path = download_and_extract(args.source_url, staging)
        emit(xml_path, args.output_dir, args.source_url)


if __name__ == "__main__":
    main()
