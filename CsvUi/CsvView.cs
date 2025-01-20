namespace CsvUi;

using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class CsvView : IBindingList, INotifyPropertyChanged
{
    private readonly RowView[] rowViews;
    private ColumnDescriptor labelColumn;

    public CsvView(ImmutableArray<ImmutableArray<string>> rows)
    {
        var descriptors = CreateDescriptors(rows[0]);
        this.Columns = new ReadOnlyObservableCollection<ColumnDescriptor>(new ObservableCollection<ColumnDescriptor>(descriptors));
        this.labelColumn = this.Columns[^1];

        var propertyDescriptorCollection = new PropertyDescriptorCollection(descriptors);
        this.rowViews = rows[1..].Select((x, i) => new RowView(x, propertyDescriptorCollection, i + 1)).ToArray();

        static ColumnDescriptor[] CreateDescriptors(ImmutableArray<string> headers)
        {
            var descriptors = new ColumnDescriptor[headers.Length];
            for (var i = 0; i < headers.Length; i++)
            {
                descriptors[i] = new ColumnDescriptor(headers[i].Trim(' ').Replace('/', '_'), i);
            }

            return descriptors;
        }
    }

    public event ListChangedEventHandler? ListChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ReadOnlyObservableCollection<ColumnDescriptor> Columns { get; }

    public bool AllowEdit => false;

    public bool AllowNew => false;

    public bool AllowRemove => false;

    public bool IsSorted => false;

    public ListSortDirection SortDirection => throw new NotSupportedException();

    public PropertyDescriptor? SortProperty => throw new NotSupportedException();

    public bool SupportsChangeNotification => false;

    public bool SupportsSearching => false;

    public bool SupportsSorting => false;

    public bool IsFixedSize => true;

    public bool IsReadOnly => true;

    public int Count => this.rowViews.Length;

    public bool IsSynchronized => throw new NotSupportedException();

    public object SyncRoot => throw new NotSupportedException();

    public ColumnDescriptor LabelColumn
    {
        get => this.labelColumn;
        set
        {
            if (value == this.labelColumn)
            {
                return;
            }

            this.labelColumn = value;
            this.OnPropertyChanged();
        }
    }

    public object? this[int index]
    {
        get => this.rowViews[index];
        set => throw new NotSupportedException();
    }

    public ColumnDescriptor FeatureColumn(int index)
    {
        if (index < this.labelColumn.Index)
        {
            return this.Columns[index];
        }

        return this.Columns[index + 1];
    }

    public int Add(object? value) => throw new NotSupportedException();

    public void AddIndex(PropertyDescriptor property) => throw new NotSupportedException();

    public object? AddNew() => throw new NotSupportedException();

    public void ApplySort(PropertyDescriptor property, ListSortDirection direction) => throw new NotSupportedException();

    public void Clear() => throw new NotSupportedException();

    public bool Contains(object? value) => this.rowViews.Contains((RowView?)value);

    public void CopyTo(Array array, int index) => this.rowViews.CopyTo(array, index);

    public int Find(PropertyDescriptor property, object key) => throw new NotSupportedException();

    public IEnumerator GetEnumerator() => this.rowViews.GetEnumerator();

    public IReadOnlyList<RowView> RowViews() => this.rowViews;

    public int IndexOf(object? value) => Array.IndexOf(this.rowViews, (RowView?)value);

    public void Insert(int index, object? value) => throw new NotSupportedException();

    public void Remove(object? value) => throw new NotSupportedException();

    public void RemoveAt(int index) => throw new NotSupportedException();

    public void RemoveIndex(PropertyDescriptor property) => throw new NotSupportedException();

    public void RemoveSort() => throw new NotSupportedException();

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class ColumnDescriptor(string name, int index)
        : PropertyDescriptor(name, null)
    {
        public int Index { get; } = index;

        public override Type ComponentType => typeof(RowView);

        public override bool IsReadOnly => true;

        public override Type PropertyType => typeof(string);

        public override object? GetValue(object? component) => ((RowView?)component)?[this.Index];

        public override void SetValue(object? component, object? value) => throw new NotSupportedException();

        public override bool CanResetValue(object component) => false;

        public override void ResetValue(object component)
        {
        }

        public override bool ShouldSerializeValue(object component) => false;

        private string GetDebuggerDisplay() => $"{this.DisplayName}[{this.Index}]";
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class RowView(ImmutableArray<string> row, PropertyDescriptorCollection properties, int rowNumber) : ICustomTypeDescriptor
    {
        public int RowNumber { get; } = rowNumber;

        public string this[int index] => row[index];

        public AttributeCollection GetAttributes() => AttributeCollection.Empty;

        public string? GetClassName() => nameof(RowView);

        public string? GetComponentName() => nameof(CsvView);

        public TypeConverter? GetConverter() => null;

        public EventDescriptor? GetDefaultEvent() => null;

        public PropertyDescriptor? GetDefaultProperty() => null;

        public object? GetEditor(Type editorBaseType) => null;

        public EventDescriptorCollection GetEvents() => EventDescriptorCollection.Empty;

        public EventDescriptorCollection GetEvents(Attribute[]? attributes) => EventDescriptorCollection.Empty;

        public PropertyDescriptorCollection GetProperties() => properties;

        public PropertyDescriptorCollection GetProperties(Attribute[]? attributes) => properties;

        public object? GetPropertyOwner(PropertyDescriptor? pd) => row;

        private string GetDebuggerDisplay() => $"[{this.RowNumber}] {string.Join(", ", row)}";
    }
}
