// A deliberately vulnerable proxy endpoint used by the finding-grouping
// worked sample (docs/ai/grouping-findings.md). Two raw findings live here -
// a missing authentication check and an unvalidated outbound fetch - which a
// synthesizing skill clusters into one higher-level SSRF vulnerability.
export async function proxy(req: Request): Promise<Response> {
  const target = new URL(req.url).searchParams.get("target"); // unvalidated input
  // No authentication gate guards this public ingress (CWE-306).
  return await fetch(target!);                                 // SSRF sink (CWE-918)
}