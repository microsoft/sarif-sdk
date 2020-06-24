// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

using BSOA.Model;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace BSOA.Generator.Templates
{
    /// <summary>
    ///  GENERATED: BSOA Entity for 'Team'
    /// </summary>
    [GeneratedCode("BSOA.Generator", "0.5.0")]
    public partial class Team : PropertyBagHolder, ISarifNode, IRow
    {
        private TeamTable _table;
        private int _index;

        public Team() : this(CompanyDatabase.Current.Team)
        { }

        public Team(Company root) : this(root.Database.Team)
        { }

        internal Team(TeamTable table) : this(table, table.Count)
        {
            table.Add();
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

        public Team(Team other) 
            : this(CompanyDatabase.Current.Team)
        {
            // <OtherAssignmentList>
            //  <OtherAssignment>
            Id = other.Id;
            //  </OtherAssignment>
            JoinPolicy = other.JoinPolicy;
            Owner = other.Owner;
            Employees = other.Employees;
            // </OtherAssignmentList>
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

        #region IEquatable<Team>
        public bool Equals(Team other)
        {
            if (other == null) { return false; }

            // <EqualsList>
            //  <Equals>
            if (this.Id != other.Id) { return false; }
            //  </Equals>
            if (this.JoinPolicy != other.JoinPolicy) { return false; }
            if (this.Owner != other.Owner) { return false; }
            if (this.Employees != other.Employees) { return false; }
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

                if (Employees != default(IList<Employee>))
                {
                    result = (result * 31) + Employees.GetHashCode();
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
        ITable IRow.Table => _table;
        int IRow.Index => _index;

        void IRow.Next()
        {
            _index++;
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
        #endregion

        public static IEqualityComparer<Team> ValueComparer => EqualityComparer<Team>.Default;
        public bool ValueEquals(Team other) => Equals(other);
        public int ValueGetHashCode() => GetHashCode();
    }
}
