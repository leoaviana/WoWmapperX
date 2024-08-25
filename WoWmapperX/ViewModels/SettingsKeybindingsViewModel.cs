using Avalonia.Media.Imaging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace WoWmapperX.ViewModels.SettingsKeybindingsViewModel
{
    class ViewModel : ReactiveObject
    {
        private ObservableCollection<Item> _items;

        public ViewModel()
        {
            Items = new ObservableCollection<Item>();
        }

        public ObservableCollection<Item> Items
        {
            get => _items;
            set => this.RaiseAndSetIfChanged(ref _items, value);
        }

        private Item _selectedItem;
        public Item SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if (_selectedItem != null)
                    _selectedItem.SelectedOpacity = 0;

                _selectedItem = value;
                if (_selectedItem != null)
                    _selectedItem.SelectedOpacity = .4;
            }
        }

        public void AddItem(int index, KeybindListItem item)
        {
            Items.Insert(index, new Item(index) { Image = item.Image, BindType = item.BindType, Key = item.Key, Name = item.Name });
        }

        public void Empty()
        {
            Items = new ObservableCollection<Item>();
            SelectedItem = null; 
        }

        public void AddRange(IEnumerable<KeybindListItem> Items)
        {
            int index = 0;
            foreach (var item in Items)
            {
                AddItem(index, item);
                index++;
            }
        }
    }
    public class Item : ReactiveObject
    {
        private double _opacity = double.NaN;
        public Item(int index) => Index = index;
        public int Index { get; }

        public Double SelectedOpacity
        {
            get => _opacity;
            set => this.RaiseAndSetIfChanged(ref _opacity, value);
        }
        public Bitmap Image { get; set; }
        public string BindType { get; set; }
        public string Key { get; set; }

        public string Name { get; set; }
    }

    public class KeybindListItem : ReactiveObject
    {
        public Double SelectedOpacity { get { return 0; } }
        public Bitmap Image { get; set; }
        public string BindType { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
    }
}
