namespace CsvUi;

using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class CsvView : IBindingList, ITypedList, INotifyPropertyChanged
{
    private readonly RowView[] rowViews;
    private readonly PropertyDescriptorCollection pdc;

    private ListSortDirection sortDirection;
    private PropertyDescriptor? sortProperty;

    public CsvView(ImmutableArray<ImmutableArray<string>> rows)
    {
        var descriptors = CreateDescriptors(rows[0]);
        this.Columns = new ReadOnlyObservableCollection<ColumnDescriptor>(new ObservableCollection<ColumnDescriptor>(descriptors));

        this.pdc = new PropertyDescriptorCollection(descriptors);
        this.rowViews = rows[1..].Select((x, i) => new RowView(x, this.pdc, i + 1)).ToArray();

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

    public bool IsSorted => this.sortProperty is not null;

    public ListSortDirection SortDirection
    {
        get => this.sortDirection;
        private set
        {
            if (value == this.sortDirection)
            {
                return;
            }

            this.sortDirection = value;
            this.OnPropertyChanged();
        }
    }

    public PropertyDescriptor? SortProperty
    {
        get => this.sortProperty;
        private set
        {
            if (ReferenceEquals(value, this.sortProperty))
            {
                return;
            }

            this.sortProperty = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.IsSorted));
        }
    }

    public bool SupportsChangeNotification => false;

    public bool SupportsSearching => false;

    public bool SupportsSorting => true;

    public bool IsFixedSize => true;

    public bool IsReadOnly => true;

    public int Count => this.rowViews.Length;

    public bool IsSynchronized => throw new NotSupportedException();

    public object SyncRoot => throw new NotSupportedException();

    public object? this[int index]
    {
        get => this.rowViews[index];
        set => throw new NotSupportedException();
    }

    public int Add(object? value) => throw new NotSupportedException();

    public void AddIndex(PropertyDescriptor property) => throw new NotSupportedException();

    public object? AddNew() => throw new NotSupportedException();

    public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
    {
        this.SortProperty = property;
        this.SortDirection = direction;
        var index = ((ColumnDescriptor)property).Index;
        var sign = direction == ListSortDirection.Descending ? 1 : -1;
        Array.Sort(this.rowViews, (x, y) => Compare(x, y));
        this.ListChanged?.Invoke(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
        int Compare(RowView x, RowView y)
        {
            return sign * string.Compare(x[index], y[index]);
        }
    }

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

    public void RemoveSort()
    {
        this.SortProperty = null;
        this.SortDirection = default;
        Array.Sort(this.rowViews, (x, y) => x.RowNumber.CompareTo(y.RowNumber));
        this.ListChanged?.Invoke(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
    }

    public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[]? listAccessors)
    {
        if (listAccessors is null)
        {
            return this.pdc;
        }

        throw new NotSupportedException();
    }

    public string GetListName(PropertyDescriptor[]? listAccessors) => throw new NotSupportedException();

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
