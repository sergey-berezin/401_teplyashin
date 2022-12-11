# 401_teplyashin
Иван Тепляшин - лабораторные работы  
Для работы необходимо скачать ONNX модель: https://github.com/onnx/models/blob/main/vision/body_analysis/emotion_ferplus/model/emotion-ferplus-7.onnx 
и поместить ее в папку library.

WpfApp_lab3:  
Для работы необходимо указать свой путь к ImageDataBase.db в файле MainWindow.xaml.cs, строка 37

lab4:  
Сервер запускается на порту 7200  
Для работы нужно создать базу данных:  
    
    dotnet ef migrations add "Initial"  
    dotnet ef database update
