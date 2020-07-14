// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

using BSOA.Collections;
using BSOA.Model;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace BSOA.Generator.Templates
{
    /// <summary>
    ///  GENERATED: BSOA Entity for 'Team'
    /// </summary>
    [GeneratedCode("BSOA.Generator", "0.5.0")]
    public partial class Team : PropertyBagHolder, ISarifNode, IRow<Team>, IEquatable<Team>
    {
        private readonly TeamTable _table;
        private readonly int _index;

        public Team() : this(CompanyDatabase.Current.Team)
        { }

        public Team(Company root) : this(root.Database.Team)
        { }

        public Team(Team other) : this(CompanyDatabase.Current.Team)
        {
            CopyFrom(other);
        }

        public Team(Company root, Team other) : this(root.Database.Team)
        {
            CopyFrom(other);
        }

        internal Team(CompanyDatabase database, Team other) : this(database.Team)
        {
            CopyFrom(other);
        }

        internal Team(TeamTable table) : this(table, table.Add()._index)
        {
            Init();
        }

        internal Team(TeamTable table, int index)
        {
            this._table = table;
            this._index = index;
        }

        public Team(
            // <ArgumentList>
            //  <Argument>
            long id,
            //  </Argument>
            SecurityPolicy joinPolicy,
            Employee owner,
            IList<Employee> members
        // </ArgumentList>
        ) 
            : this(CompanyDatabase.Current.Team)
        {
            // <AssignmentList>
            //  <Assignment>
            Id = id;
            //  </Assignment>
            WhenFormed = whenFormed;
            JoinPolicy = joinPolicy;
            Attributes = attributes;
            Owner = owner;
            Employees = members;
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
            get => TypedList<Employee>.Get(_table.Database.Employee, _table.Members, _index);
            set => TypedList<Employee>.Set(_table.Database.Employee, _table.Members, _index, value);
        }

        //   </RefListColumn>
        // </ColumnList>

        #region IEquatable<Team>
        public bool Equals(Team other)
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
            return Equals(obj as Team);
        }

        public static bool operator ==(Team left, Team right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(Team left, Team right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return !object.ReferenceEquals(right, null);
            }

            return !left.Equals(right);
        }
        #endregion

        #region IRow
        ITable IRow<Team>.Table => _table;
        int IRow<Team>.Index => _index;

        public void CopyFrom(Team other)
        {
            // <OtherAssignmentList>
            //  <OtherAssignment>
            Id = other.Id;
            //  </OtherAssignment>
            JoinPolicy = other.JoinPolicy;
            //  <RefOtherAssignment>
            Owner = Employee.DeepClone(_table.Database, other.Owner);
            //  </RefOtherAssignment>
            //  <RefListOtherAssignment>
            Members = other.Members?.Select((item) => Employee.DeepClone(_table.Database, item)).ToList();
            //  </RefListOtherAssignment>
            // </OtherAssignmentList>
        }
        #endregion

        #region ISarifNode
        public SarifNodeKind SarifNodeKind => SarifNodeKind.Team;

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Team DeepClone()
        {
            return (Team)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Team(this);
        }

        internal static Team DeepClone(CompanyDatabase db, Team other)
        {
            return (other == null ? null : new Team(db, other));
        }
        #endregion

        public static IEqualityComparer<Team> ValueComparer => EqualityComparer<Team>.Default;
        public bool ValueEquals(Team other) => Equals(other);
        public int ValueGetHashCode() => GetHashCode();
    }
}
