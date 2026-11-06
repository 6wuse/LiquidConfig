using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiquidConfig.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiquidConfig.App.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ViewModel1 _viewModel1;
        private readonly ViewModel2 _viewModel2;
        private readonly ViewModel3 _viewModel3;

        [ObservableProperty]
        private object _currentViewModel;//当前显示的界面

        public MainViewModel(ViewModel1 viewModel1, ViewModel2 viewModel2, ViewModel3 viewModel3)
        {
            _viewModel1 = viewModel1;
            _viewModel2 = viewModel2;
            _viewModel3 = viewModel3;
            _currentViewModel = _viewModel1; //默认显示第一个界面
        }
        [RelayCommand]
        private void Navigate(string page)
        {
            CurrentViewModel = page switch
            {
                "View1" => _viewModel1,
                "View2" => _viewModel2,
                "View3" => _viewModel3,
                    _   => _viewModel1,
            };
        }
    }
}
