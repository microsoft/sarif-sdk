// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

using BSOA.IO;
using BSOA.Model;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace BSOA.Generator.Templates
{
    /// <summary>
    ///  GENERATED: BSOA Root Entity for 'Company'
    /// </summary>
    [GeneratedCode("BSOA.Generator", "0.5.0")]
    public partial class Company : PropertyBagHolder, ISarifNode, IRow<Company>, IEquatable<Company>
    {
        private readonly CompanyTable _table;
        private readonly int _index;

        internal CompanyDatabase Database => _table.Database;
        public ITreeSerializable DB => _table.Database;

        public Company() : this(new CompanyDatabase().Company)
        { }
        
        public Company(Company other) : this(new CompanyDatabase().Company)
        {
            CopyFrom(other);
        }

        internal Company(CompanyTable table) : this(table, table.Add()._index)
        {
            Init();
        }

        internal Company(CompanyTable table, int index)
        {
            this._table = table;
            this._index = index;
        }

        public Company(
            // <ArgumentList>
            //  <Argument>
            long id,
            //  </Argument>
            SecurityPolicy joinPolicy,
            Employee owner,
            IList<Employee> members
        // </ArgumentList>
        ) : this()
        {
            // <AssignmentList>
            //  <Assignment>
            Id = id;
            //  </Assignment>
            WhenFormed = whenFormed;
            JoinPolicy = joinPolicy;
            Attributes = attributes;
            Owner = owner;
            Members = members;
            // </AssignmentList>
        }

        partial void Init();

        // <ColumnList>
        //   <SimpleColumn>
        public virtual long Id
        {
            get => _table.Id[_index];
            set => _table.Id[_index] = value;
        }

        //   </SimpleColumn>
        //   <EnumColumn>
        public virtual SecurityPolicy JoinPolicy
        {
            get => (SecurityPolicy)_table.JoinPolicy[_index];
            set => _table.JoinPolicy[_index] = (byte)value;
        }

        //   </EnumColumn>
        //   <RefColumn>
        public virtual Employee Owner
        {
            get => _table.Database.Employee.Get(_table.Owner[_index]);
            set => _table.Owner[_index] = _table.Database.Employee.LocalIndex(value);
        }

        //   </RefColumn>
        //   <RefListColumn>
        public virtual IList<Employee> Members
        {
            get => _table.Database.Employee.List(_table.Members[_index]);
            set => _table.Database.Employee.List(_table.Members[_index]).SetTo(value);
        }

        //   </RefListColumn>
        // </ColumnList>

        #region IEquatable<Company>
        public bool Equals(Company other)
        {
            if (other == null) { return false; }

            // <EqualsList>
            //  <Equals>
            if (!object.Equals(this.Id, other.Id)) { return false; }
            //  </Equals>
            if (!object.Equals(this.JoinPolicy, other.JoinPolicy)) { return false; }
            if (!object.Equals(this.Owner, other.Owner)) { return false; }
            if (!object.Equals(this.Members, other.Members)) { return false; }
            // </EqualsList>

            return true;
        }
        #endregion

        #region Object overrides
        public override int GetHashCode()
        {
            int result = 17;

            unchecked
            {
                // <GetHashCodeList>
                //  <GetHashCode>
                if (Id != default(long))
                {
                    result = (result * 31) + Id.GetHashCode();
                }

                //  </GetHashCode>

                if (JoinPolicy != default(SecurityPolicy))
                {
                    result = (result * 31) + JoinPolicy.GetHashCode();
                }

                if (Owner != default(Employee))
                {
                    result = (result * 31) + Owner.GetHashCode();
                }

                if (Members != default(IList<Employee>))
                {
                    result = (result * 31) + Members.GetHashCode();
                }
                // </GetHashCodeList>
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Company);
        }

        public static bool operator ==(Company left, Company right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(Company left, Company right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return !object.ReferenceEquals(right, null);
            }

            return !left.Equals(right);
        }
        #endregion

        #region IRow
        ITable IRow<Company>.Table => _table;
        int IRow<Company>.Index => _index;

        public void CopyFrom(Company other)
        {
            // <OtherAssignmentList>
            //  <OtherAssignment>
            Id = other.Id;
            //  </OtherAssignment>
            JoinPolicy = other.JoinPolicy;
            Owner = other.Owner;
            Members = other.Members;
            // </OtherAssignmentList>
        }
        #endregion

        #region ISarifNode
        public SarifNodeKind SarifNodeKind => SarifNodeKind.Company;

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Company DeepClone()
        {
            return (Company)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Company(this);
        }
        #endregion

        #region Easy Serialization
        public void WriteBsoa(System.IO.Stream stream)
        {
            using (BinaryTreeWriter writer = new BinaryTreeWriter(stream))
            {
                DB.Write(writer);
            }
        }

        public void WriteBsoa(string filePath)
        {
            WriteBsoa(System.IO.File.Create(filePath));
        }

        public static Company ReadBsoa(System.IO.Stream stream)
        {
            using (BinaryTreeReader reader = new BinaryTreeReader(stream))
            {
                Company result = new Company();
                result.DB.Read(reader);
                return result;
            }
        }

        public static Company ReadBsoa(string filePath)
        {
            return ReadBsoa(System.IO.File.OpenRead(filePath));
        }

        public static TreeDiagnostics Diagnostics(string filePath)
        {
            return Diagnostics(System.IO.File.OpenRead(filePath));
        }

        public static TreeDiagnostics Diagnostics(System.IO.Stream stream)
        {
            using (BinaryTreeReader btr = new BinaryTreeReader(stream))
            using (TreeDiagnosticsReader reader = new TreeDiagnosticsReader(btr))
            {
                Company result = new Company();
                result.DB.Read(reader);
                return reader.Tree;
            }
        }
        #endregion

        public static IEqualityComparer<Company> ValueComparer => EqualityComparer<Company>.Default;
        public bool ValueEquals(Company other) => Equals(other);
        public int ValueGetHashCode() => GetHashCode();
    }
}
