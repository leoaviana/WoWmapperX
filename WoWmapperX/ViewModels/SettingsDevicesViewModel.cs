using Avalonia.Media.Imaging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WoWmapperX.ViewModels.SettingsDevicesViewModel
{

    public class ViewModel : ReactiveObject
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
        public Item SelectedItem { 
            get { 
                return _selectedItem; 
            } 
            set 
            { 
                if(_selectedItem != null)
                    _selectedItem.SelectedOpacity = 0;

                _selectedItem = value; 
                if(_selectedItem != null)
                _selectedItem.SelectedOpacity = .4;
            } 
        } 

        public void AddItem(int index, ControllerListItem item)
        { 
            Items.Insert(index, new Item(index) { Image = item.Image, Name = item.Name, Type = item.Type});
        }

        public void Empty()
        {
            Items = new ObservableCollection<Item>();
        }

        public void AddRange(IEnumerable<ControllerListItem> Items)
        {
            int index = 0;
            foreach(var item in Items)
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
        public Item() { }

        public virtual int Index { get; }

        public virtual Double SelectedOpacity
        {
            get => _opacity;
            set => this.RaiseAndSetIfChanged(ref _opacity, value);
        }
        public virtual Bitmap Image { get; set; }
        public virtual string Type { get; set; }
        public virtual string Name { get; set; }
    }

    public class ControllerListItem : Item
    {
        public override int Index => base.Index;
        public override Double SelectedOpacity { get { return 0; } }
        public override Bitmap Image { get; set; }
        public override string Type { get; set; }
        public override string Name { get; set; }
    }
}
