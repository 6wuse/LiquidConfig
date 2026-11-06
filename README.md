这是一个C#开发的项目模板
使用Host托管理WPF,并且采用了MVVM框架。
开发者只需要创建接口和对应的实现即可不断丰富其功能。

如果需要增加界面，请完成一下操作：
1：在Views和ViewModels文件夹中创建对应的文件（xmal和cs）
2：在App.cs中注册对应的界面服务（直接复制已有的即可）
3：在MainWindow.xaml 增加对应的按键
4：MainViewModel.cs中增加对应的界面
5：App.xaml增加对应的修饰（第一步创建的xmal和cs）

即按照模板在上面文件夹中创建即可，模板中已经给出了三个示例
