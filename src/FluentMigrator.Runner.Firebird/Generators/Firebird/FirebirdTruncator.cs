﻿#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Model;
using System.Security.Cryptography;

namespace FluentMigrator.Runner.Generators.Firebird
{
    public class FirebirdTruncator
    {
        private readonly bool _enabled;
        private readonly bool _packKeyNames;

        public FirebirdTruncator(bool enabled, bool packKeyNames)
        {
            _enabled = enabled;
            _packKeyNames = packKeyNames;
        }

        public void Truncate(CreateSchemaExpression expression) { }
        public void Truncate(AlterSchemaExpression expression) { }
        public void Truncate(DeleteSchemaExpression expression) { }

        public void Truncate(CreateTableExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
            TruncateColumns(expression.Columns);
        }

        public void Truncate(AlterTableExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
        }

        public void Truncate(DeleteTableExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
        }

        public void Truncate(RenameTableExpression expression)
        {
            expression.OldName = Truncate(expression.OldName);
            expression.NewName = Truncate(expression.NewName);
        }

        public void Truncate(ColumnDefinition column)
        {
            column.Name = Truncate(column.Name);
            column.TableName = Truncate(column.TableName);
            if (column.IsPrimaryKey)
                column.PrimaryKeyName = _packKeyNames ? Pack(column.PrimaryKeyName) : Truncate(column.PrimaryKeyName);
        }

        public void Truncate(CreateColumnExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
            Truncate(expression.Column);
        }

        public void Truncate(AlterColumnExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
            Truncate(expression.Column);
        }

        public void Truncate(DeleteColumnExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
            expression.ColumnNames = TruncateNames(expression.ColumnNames);
        }

        public void Truncate(RenameColumnExpression expression)
        {
            expression.OldName = Truncate(expression.OldName);
            expression.NewName = Truncate(expression.NewName);
            expression.TableName = Truncate(expression.TableName);
        }

        public void Truncate(IndexDefinition index)
        {
            index.TableName = Truncate(index.TableName);
            index.Name = _packKeyNames ? Pack(index.Name) : Truncate(index.Name);
            index.Columns.ToList().ForEach(x => x.Name = Truncate(x.Name));
        }


        public void Truncate(CreateIndexExpression expression)
        {
            Truncate(expression.Index);
        }

        public void Truncate(DeleteIndexExpression expression)
        {
            Truncate(expression.Index);
        }

        public void Truncate(ConstraintDefinition constraint)
        {
            constraint.TableName = Truncate(constraint.TableName);
            constraint.ConstraintName = _packKeyNames ? Pack(constraint.ConstraintName) : Truncate(constraint.ConstraintName);
            constraint.Columns = TruncateNames(constraint.Columns);
        }

        public void Truncate(CreateConstraintExpression expression)
        {
            Truncate(expression.Constraint);
        }

        public void Truncate(DeleteConstraintExpression expression)
        {
            Truncate(expression.Constraint);
        }

        public void Truncate(ForeignKeyDefinition foreignKey)
        {
            foreignKey.Name = _packKeyNames ? Pack(foreignKey.Name) : Truncate(foreignKey.Name);
            foreignKey.PrimaryTable = Truncate(foreignKey.PrimaryTable);
            foreignKey.PrimaryColumns = TruncateNames(foreignKey.PrimaryColumns);
            foreignKey.ForeignTable = Truncate(foreignKey.ForeignTable);
            foreignKey.ForeignColumns = TruncateNames(foreignKey.ForeignColumns);
        }

        public void Truncate(CreateForeignKeyExpression expression)
        {
            Truncate(expression.ForeignKey);
        }
        public void Truncate(DeleteForeignKeyExpression expression)
        {
            Truncate(expression.ForeignKey);
        }

        public void Truncate(AlterDefaultConstraintExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
            expression.ColumnName = Truncate(expression.ColumnName);
        }

        public void Truncate(DeleteDefaultConstraintExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
            expression.ColumnName = Truncate(expression.ColumnName);
        }

        public void Truncate(SequenceDefinition sequence)
        {
            sequence.Name = Truncate(sequence.Name);
        }

        public void Truncate(CreateSequenceExpression expression)
        {
            Truncate(expression.Sequence);
        }

        public void Truncate(DeleteSequenceExpression expression)
        {
            expression.SequenceName = Truncate(expression.SequenceName);
        }

        public void Truncate(InsertDataExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
            List<InsertionDataDefinition> insertions = new List<InsertionDataDefinition>();
            foreach (InsertionDataDefinition insertion in expression.Rows)
            {
                InsertionDataDefinition newInsertion = new InsertionDataDefinition();
                foreach (var data in insertion)
                {
                    newInsertion.Add(new KeyValuePair<string, object>(Truncate(data.Key), data.Value));
                }
                insertions.Add(newInsertion);
            }
            expression.Rows.Clear();
            expression.Rows.AddRange(insertions);
        }

        public void Truncate(DeleteDataExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
            List<DeletionDataDefinition> deletions = new List<DeletionDataDefinition>();
            foreach (DeletionDataDefinition deletion in expression.Rows)
            {
                DeletionDataDefinition newDeletion = new DeletionDataDefinition();
                foreach (var data in deletion)
                {
                    newDeletion.Add(new KeyValuePair<string, object>(Truncate(data.Key), data.Value));
                }
                deletions.Add(newDeletion);
            }
            expression.Rows.Clear();
            expression.Rows.AddRange(deletions);
        }

        public void Truncate(UpdateDataExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
            List<KeyValuePair<string, object>> newSet = new List<KeyValuePair<string, object>>();
            foreach (var data in expression.Set)
            {
                newSet.Add(new KeyValuePair<string, object>(Truncate(data.Key), data.Value));
            }
            expression.Set.Clear();
            expression.Set.AddRange(newSet);
            if (!expression.IsAllRows)
            {
                List<KeyValuePair<string, object>> newWhere = new List<KeyValuePair<string, object>>();
                foreach (var data in expression.Where)
                {
                    newWhere.Add(new KeyValuePair<string, object>(Truncate(data.Key), data.Value));
                }
                expression.Where.Clear();
                expression.Where.AddRange(newWhere);
            }
        }

        public ICollection<string> TruncateNames(ICollection<string> names)
        {
            List<string> ret = new List<string>();
            foreach (string item in names)
            {
                ret.Add(Truncate(item));
            }
            return ret;
        }

        public void TruncateColumns(ICollection<ColumnDefinition> columns)
        {
            foreach (ColumnDefinition colDef in columns)
            {
                Truncate(colDef);
            }
        }

        public string Truncate(string name)
        {
            if (!String.IsNullOrEmpty(name))
            {
                if (name.Length > FirebirdOptions.MaxNameLength)
                {
                    if (!_enabled)
                        throw new ArgumentException(String.Format("Name too long: {0}", name));

                    return name.Substring(0, Math.Min(FirebirdOptions.MaxNameLength, name.Length));
                }
            }
            return name;
        }

        public string Pack(string name)
        {
            if (!String.IsNullOrEmpty(name))
            {
                if (name.Length > FirebirdOptions.MaxNameLength)
                {
                    if (!_enabled)
                        throw new ArgumentException(String.Format("Name too long: {0}", name));

                    byte[] byteHash = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(name));
                    string hash = Convert.ToBase64String(byteHash);
                    StringBuilder sb = new StringBuilder(hash.Length);
                    int hLength = hash.Length;
                    for (int i = 0; i < hLength; i++)
                    {
                        char c = hash[i];
                        if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                            sb.Append(c);
                    }
                    hash = sb.ToString();
                    return String.Format("fk_{0}", hash.Substring(0, Math.Min(28, hash.Length)));
                }
            }
            return name;

        }
    }
}
