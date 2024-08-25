using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using WoWmapperX.Controllers;
using WoWmapperX.ViewModels;
using SettingsDevicesViewModel = WoWmapperX.ViewModels.SettingsDevicesViewModel;

namespace WoWmapperX.SettingsPanels
{
    public partial class SettingsDevices : UserControl
    {
        private Bitmap _ds4Image;
        private Bitmap _xinputImage;
        private List<IController> _controllers;

        private readonly SettingsDevicesViewModel.ViewModel _viewModel;

        public SettingsDevices()
        {
            DataContext = _viewModel = new SettingsDevicesViewModel.ViewModel();
            InitializeComponent();

            _ds4Image = new Bitmap(AssetLoader.Open(new System.Uri("avares://WoWmapperX/Resources/ds4.png")));
            _xinputImage = new Bitmap(AssetLoader.Open(new System.Uri("avares://WoWmapperX/Resources/xinput.png")));

            ControllerManager.ControllersChanged += UpdateControllerList;
            ControllerManager.ActiveControllerChanged += UpdateSelectedController;
            UpdateControllerList();
            UpdateSelectedController(ControllerManager.ActiveController);

            ListAvailableDevices.PointerPressed += ListAvailableDevices_Click;
            ListAvailableDevices.PointerMoved += ListAvailableDevices_PointerOver;
            ListAvailableDevices.PointerExited += ListAvailableDevices_PointerLeave;
        }

        #region "'ListView' stuff"
        private void ListAvailableDevices_Click(object sender, PointerPressedEventArgs e)
        {
            object item = (e.Source as Control)?.DataContext; 
            _viewModel.SelectedItem = (SettingsDevicesViewModel.Item)item;
        }


        object oldItem = null;
        private void ListAvailableDevices_PointerOver(object sender, PointerEventArgs e)
        {
            object item = (e.Source as Control)?.DataContext;
            if(oldItem != null)
            {
                if(_viewModel.SelectedItem == null)
                {
                    ((SettingsDevicesViewModel.Item)oldItem).SelectedOpacity = 0; 
                }
                else if (_viewModel.SelectedItem.Index != ((SettingsDevicesViewModel.Item)oldItem).Index)
                    ((SettingsDevicesViewModel.Item)oldItem).SelectedOpacity = 0;
                oldItem = null;
            }
            try
            {
                if(_viewModel.SelectedItem == null)
                {
                    ((SettingsDevicesViewModel.Item)item).SelectedOpacity = 0.2;
                    oldItem = item;
                }
                else if (_viewModel.SelectedItem.Index != ((SettingsDevicesViewModel.Item)item).Index)
                {
                    ((SettingsDevicesViewModel.Item)item).SelectedOpacity = 0.2;
                    oldItem = item;
                }
            }
            catch { }
        } 

        private void ListAvailableDevices_PointerLeave(object sender, PointerEventArgs e)
        {
            if (oldItem != null)
            {
                if (_viewModel.SelectedItem == null)
                {
                    ((SettingsDevicesViewModel.Item)oldItem).SelectedOpacity = 0;
                }
                else if (_viewModel.SelectedItem.Index != ((SettingsDevicesViewModel.Item)oldItem).Index)
                    ((SettingsDevicesViewModel.Item)oldItem).SelectedOpacity = 0;
                oldItem = null;
            }
        }

        #endregion

        private void UpdateSelectedController(IController controller)
        {
            if (controller == null)
            {
                ListSelectedDevice.IsVisible = false;
                TextNoController.IsVisible = true;

                return;
            }

            ListSelectedDevice.IsVisible = true;
            TextNoController.IsVisible = false;

            var item = new List<SettingsDevicesViewModel.ControllerListItem>();
            item.Add(GetListItem(controller));

            ListSelectedDevice.ItemsSource = item;
        }

        public SettingsDevicesViewModel.ControllerListItem GetListItem(IController controller)
        {
            if (controller.Type == GamepadType.PlayStation)
            {
                return new SettingsDevicesViewModel.ControllerListItem()
                {
                    Image = _ds4Image,
                    Type = controller.Type.ToString(),
                    Name = controller.Name
                };
            }
            else if (controller.Type == GamepadType.Xbox)
            {
                return new SettingsDevicesViewModel.ControllerListItem()
                {
                    Image = _xinputImage,
                    Type = "Xinput",
                    Name = controller.Name
                };
            }
            else return null;
        }

        public void UpdateControllerList()
        {
            _controllers = ControllerManager.Controllers;

            var controllerList = new List<SettingsDevicesViewModel.ControllerListItem>();

            foreach (var controller in _controllers)
            {
                if (controller.Type == GamepadType.PlayStation)
                {
                    controllerList.Add(GetListItem(controller));
                }
                else if (controller.Type == GamepadType.Xbox)
                {
                    controllerList.Add(GetListItem(controller));
                }
            }

            _viewModel.Empty();
            _viewModel.AddRange(controllerList);
        }

        public void ListAvailableDevices_OnMouseDoubleClick(object sender, TappedEventArgs e)
        {
            SetActiveController();
        }

        private void ButtonUseController_Click(object sender, RoutedEventArgs e)
        {
            SetActiveController();
        }

        private void SetActiveController()
        {
            var _fadeOutAnimation = new Animation
            {
                Children =
                {
                    new KeyFrame()
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = Visual.OpacityProperty,
                                Value = 0d
                            }
                        },
                        Cue = new Cue(1d)
                    }

                }
            };
            var _fadeInAnimation = new Animation
            {
                Children =
                {
                    new KeyFrame()
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = Visual.OpacityProperty,
                                Value = 1d
                            }
                        },
                        Cue = new Cue(1d)
                    }

                }
            };
            _fadeOutAnimation.Duration = _fadeInAnimation.Duration = new System.TimeSpan(0, 0, 0, 0, 200);

            if (_viewModel.SelectedItem != null)
            {
                /*
                _fadeOutAnimation.Apply(ListSelectedDevice, null, Observable.Return(true), () =>
                {
                    ListSelectedDevice.Opacity = 0;
                    ControllerManager.SetController(_controllers[_viewModel.SelectedItem.Index]);

                    _fadeInAnimation.Apply(ListSelectedDevice, null, Observable.Return(true), () =>
                    {
                        ListSelectedDevice.Opacity = 1;
                    });
                });
                */
                _fadeOutAnimation.RunAsync(ListSelectedDevice).ContinueWith(t =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Invoke(() =>
                    {
                        ListSelectedDevice.Opacity = 0;
                        ControllerManager.SetController(_controllers[_viewModel.SelectedItem.Index]);

                        _fadeInAnimation.RunAsync(ListSelectedDevice).ContinueWith(t =>
                        {
                            Avalonia.Threading.Dispatcher.UIThread.Invoke(() => {
                                ListSelectedDevice.Opacity = 1;
                            });
                        });
                    });
                });
            }
        }
    }
}
