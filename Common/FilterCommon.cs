using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows.Controls;

namespace Common
{
    [DataContract]
    public sealed class FilterCommon : NotifyProperty
    {
        #region Private Fields

        private bool isFiltered;

        #endregion Private Fields

        #region Public Properties

        public HashSet<object> PreviouslyFilteredItems { get; set; } = new HashSet<object>(EqualityComparer<object>.Default);

        [DataMember(Name = "FilteredItems")]
        public List<object> FilteredItems
        {
            get
            {
                return FieldType?.BaseType == typeof(Enum)
                    ? PreviouslyFilteredItems.ToList().ConvertAll(f => (object)f.ToString())
                    : PreviouslyFilteredItems?.ToList();
            }

            set => PreviouslyFilteredItems = value.ToHashSet();
        }

        [DataMember(Name = "FilterCondition")]
        public FilterCondition Condition { get; set; } = FilterCondition.None;

        [DataMember(Name = "FilterValue")]
        public string ConditionValue { get; set; }

        [DataMember(Name = "FieldName")]
        public string FieldName { get; set; }

        public Button FilterButton { get; set; }
        public Loc Translate { get; set; }

        // Use a string to store the type name for serialization
        [DataMember(Name = "FieldType")]
        private string FieldTypeString { get; set; }

        // Property to get and set the actual Type
        public Type FieldType
        {
            get
            {
                try
                {
                    return Type.GetType(FieldTypeString);
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    Debug.WriteLine($"Error deserializing type: {ex.Message}");
                    return null; // or a default type, e.g., typeof(object)
                }
            }
            set => FieldTypeString = value.AssemblyQualifiedName;
        }
        public bool IsFiltered
        {
            get => isFiltered;
            set
            {
                isFiltered = value;
                OnPropertyChanged(nameof(IsFiltered));
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        ///     Add the filter to the predicate dictionary
        /// </summary>
        public void AddFilter(Dictionary<string, Predicate<object>> criteria)
        {
            if (IsFiltered) return;

            // add to list of predicates
            criteria.Add(FieldName, Predicate);

            IsFiltered = true;
            return;

            // predicate of filter
            bool Predicate(object o)
            {
                var value = FieldType == typeof(DateTime)
                    ? ((DateTime?)o.GetPropertyValue(FieldName))?.Date
                    : o.GetPropertyValue(FieldName);

                if (PreviouslyFilteredItems.Any())
                {
                    return !PreviouslyFilteredItems.Contains(value);
                }

                if (Condition != FilterCondition.None && !string.IsNullOrEmpty(ConditionValue))
                {
                    return EvaluateCondition(value, Condition, ConditionValue);
                }

                return true;
            }
        }

        private bool EvaluateCondition(object value, FilterCondition condition, string conditionValue)
        {
            if (value == null) return false;

            try
            {
                switch (condition)
                {
                    case FilterCondition.Equals:
                        return value.Equals(Convert.ChangeType(conditionValue, FieldType));
                    case FilterCondition.NotEquals:
                        return !value.Equals(Convert.ChangeType(conditionValue, FieldType));
                    case FilterCondition.Contains when FieldType == typeof(string):
                        return ((string)value).Contains(conditionValue, StringComparison.OrdinalIgnoreCase);
                    case FilterCondition.StartsWith when FieldType == typeof(string):
                        return ((string)value).StartsWith(conditionValue, StringComparison.OrdinalIgnoreCase);
                    case FilterCondition.EndsWith when FieldType == typeof(string):
                        return ((string)value).EndsWith(conditionValue, StringComparison.OrdinalIgnoreCase);
                    case FilterCondition.GreaterThan:
                        return Comparer<object>.Default.Compare(value, Convert.ChangeType(conditionValue, FieldType)) > 0;
                    case FilterCondition.LessThan:
                        return Comparer<object>.Default.Compare(value, Convert.ChangeType(conditionValue, FieldType)) < 0;
                    case FilterCondition.GreaterThanOrEqual:
                        return Comparer<object>.Default.Compare(value, Convert.ChangeType(conditionValue, FieldType)) >= 0;
                    case FilterCondition.LessThanOrEqual:
                        return Comparer<object>.Default.Compare(value, Convert.ChangeType(conditionValue, FieldType)) <= 0;
                    default:
                        return true;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion Public Methods
    }
}
