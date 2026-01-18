// "Консольный файловый менеджер"
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("=== Консольный файловый менеджер ===");
        Console.WriteLine("Введите команду. Для списка команд введите 'help'");

        string directory = Directory.GetCurrentDirectory();

        while (true)
        {
            Console.WriteLine($"\n{directory}\n");
            try
            {
                string input = Console.ReadLine().Trim();
                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string command = parts[0];
                string[] arg = parts.Skip(1).ToArray();

                Console.WriteLine();

                switch (command)
                {
                    case "ls":
                        if (arg.Length >= 1)
                            ListDirectory(arg[0]);
                        else
                            Console.WriteLine("Необходимо указать путь");
                        break;

                    case "cp":
                        if (arg.Length >= 2)
                            CopyFile(arg[0], arg[1]);
                        else
                            Console.WriteLine("Необходимо указать источник и приемник");
                        break;

                    case "mv":
                        if (arg.Length >= 2)
                            MoveFile(arg[0], arg[1]);
                        else
                            Console.WriteLine("Необходимо указать источник и приемник");
                        break;

                    case "rm":
                        if (arg.Length >= 1)
                            DeleteFile(arg[0]);
                        else
                            Console.WriteLine("Необходимо указать путь");
                        break;

                    case "info":
                        if (arg.Length >= 1)
                            FileInfoDisplay(arg[0]);
                        else
                            Console.WriteLine("Необходимо указать путь");
                        break;

                    case "cat":
                        if (arg.Length >= 1)
                            DisplayFileContent(arg[0]);
                        else
                            Console.WriteLine("Необходимо указать путь");
                        break;

                    case "find":
                        if (arg.Length == 1)
                            FindFiles(arg[0]);
                        else if(arg.Length == 2)
                            FindFiles(arg[0], arg[1]);
                        else
                            Console.WriteLine("Необходимо указать каталог с маской или только маску");
                        break;

                    case "zip":
                        if (arg.Length >= 1)
                        {
                            string[] files = arg.Skip(1).ToArray();
                            CreateZipArchive(arg[0], files);
                        }
                        else
                            Console.WriteLine("Необходимо указать архив и файл/файлы");
                        break;

                    case "unzip":
                        if (arg.Length == 1)
                            ExtractZipArchive(arg[0]);
                        else if (arg.Length == 2)
                            ExtractZipArchive(arg[0], arg[1]);
                        else
                            Console.WriteLine("Необходимо указать архив и каталог или только архив");
                        break;

                    case "cd":
                        if (arg.Length == 1)
                            ChangeDirectory(Directory.GetCurrentDirectory(), arg[0]);
                        else
                            Console.WriteLine("Необходимо указать путь");
                        break;

                    case "md":
                        if (arg.Length == 1)
                            CreateDirectory(arg[0]);
                        else
                            Console.WriteLine("Необходимо указать путь");
                        break;

                    case "help":
                        if (arg.Length == 0)
                            DisplayHelp();
                        else
                            DisplayCommandHelp(arg[0]);
                        break;

                    case "exit":
                        return;

                    default:
                        Console.WriteLine("Неверная команда");
                        break;
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
    static void ListDirectory(string path) //Вывод содержимого каталога
    {
        if (Directory.Exists(path))
        {
            int countDirs = 0, countFiles = 0;
            long countLength = 0;

            Console.WriteLine("Папки:");
            string[] dirs = Directory.GetDirectories(path);
            Console.ForegroundColor = ConsoleColor.Blue;

            foreach (string d in dirs)
            {
                DirectoryInfo dir = new DirectoryInfo(d);
                Console.Write($"    {dir.Name}");
                countDirs++;
            }

            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Файлы:");

            string[] files = Directory.GetFiles(path);
            Console.ForegroundColor = ConsoleColor.White;

            foreach (string f in files)
            {
                FileInfo file= new FileInfo(f);
                Console.WriteLine($"   {file.Name}   ({FormatedFileSize(file.Length)} byte)   {file.CreationTime}");
                countFiles++;
                countLength += file.Length;
            }

            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine($"Всего: {countFiles} файлов/файла ({FormatedFileSize(countLength)}), {countDirs} папок/папки");
        }
        else
        {
            Console.WriteLine("Ошибка: такой папки не существует");
        }
    }
    static void CopyFile(string source, string dest) //Копирование файла
    {
        FileInfo sourceFile = new FileInfo(source);

        if(!sourceFile.Exists)
        {
            Console.WriteLine($"Файл {source} не найден");
            return;
        }
        else if (Directory.Exists(dest)) //Если путь - папка
        {
            string newFile = Path.Combine(dest, sourceFile.Name);
            if (AskOverWrite(sourceFile, newFile) == true)
                sourceFile.CopyTo(newFile, true);
            else
            {
                Console.WriteLine("Операция отменена");
                return;
            }
        }
        else
        {
            if (File.Exists(dest))
            {
                if (AskOverWrite(sourceFile, dest) == true)
                    sourceFile.CopyTo(dest, true);
                else
                {
                    Console.WriteLine("Операция отменена");
                    return;
                }
            }
            else
                sourceFile.CopyTo(dest, true);
        }
        Console.WriteLine($"Успешное копирование {sourceFile.Name} ({FormatedFileSize(sourceFile.Length)})");
    }
    static void MoveFile(string source, string dest) //Перемещение или переименование
    {
        FileInfo sourceFile = new FileInfo(source);
         
        if (!sourceFile.Exists)
        {
            Console.WriteLine($"Файл {source} не найден");
            return;
        }

        if (Directory.Exists(dest))
        {
            string newFile = Path.Combine(dest, sourceFile.Name);

            if (File.Exists(newFile))
            {
                if (AskOverWrite(sourceFile, newFile) == true)
                {
                    sourceFile.MoveTo(newFile, true);
                    Console.WriteLine($"Перезаписан: {sourceFile.Name} ({FormatedFileSize(sourceFile.Length)})");
                }
                else
                {
                    Console.WriteLine("Операция отменена");
                }
            }
            else
            {
                sourceFile.MoveTo(newFile);
                Console.WriteLine($"Перемещён файл {sourceFile.Name} {FormatedFileSize(sourceFile.Length)}");
            }
        }
        else if (File.Exists(dest))
        {
            if(AskOverWrite(sourceFile, dest) == true)
            {
                sourceFile.MoveTo(dest, true);
                Console.WriteLine($"Перезаписан: {sourceFile.Name} ({FormatedFileSize(sourceFile.Length)})");
            }
            else
            {
                Console.WriteLine("Операция отменена");
            }
        }
        else
        {
            sourceFile.MoveTo(dest);
            Console.WriteLine($"Перемещён файл {sourceFile.Name} {FormatedFileSize(sourceFile.Length)}");
        }
    }
    static void DeleteFile(string path) //Удаление файла
    {
        FileInfo file = new FileInfo(path);
        if (file.Exists)
        {
            Console.WriteLine("Удаление файла:");
            Console.WriteLine($"    Имя: {file.Name}");
            Console.WriteLine($"    Размер: {FormatedFileSize(file.Length)}");
            Console.WriteLine($"    Создан: {file.CreationTime}");
            Console.WriteLine($"    Изменён: {file.LastWriteTime}");

            Console.WriteLine("\nВы уверены(+ / -) ?");
            string command = Console.ReadLine().Trim();
            Console.WriteLine();

            if(command == "+")
            {
                File.Delete(path);
                Console.WriteLine($"Файл {path} успешно удалён");
            }
            else
            {
                Console.WriteLine("Операция отменена");
            }
        }
        else 
        { 
            Console.WriteLine($"Файл {path} не найден");
        }
    }
    static void FileInfoDisplay(string path) //Информация о файле
    {
        FileInfo file = new FileInfo(path);

        if(file.Exists) 
        {
            Console.WriteLine("ДЕТАЛЬНАЯ ИНФОРМАЦИЯ О ФАЙЛЕ\nОСНОВНОЕ\n");
            Console.WriteLine($"    Имя:    {file.Name}");
            Console.WriteLine($"    Полный путь:    {Path.GetFullPath(path)}");
            Console.WriteLine($"    Расширение:    {file.Extension}");
            Console.WriteLine($"    Тип:    {GetFileType(path)}");
            Console.WriteLine($"    Размер:    {FormatedFileSize(file.Length)} ({file.Length} байт)\n");

            Console.WriteLine("ДАТЫ");
            Console.WriteLine($"    Создан:    {file.CreationTime}");
            Console.WriteLine($"    Изменён:    {file.LastWriteTime}");
            Console.WriteLine($"    Открыт:    {file.LastAccessTime}\n");

            Console.WriteLine("АТРИБУТЫ");
            Console.WriteLine($"    Только чтение:    {(file.Attributes.HasFlag(FileAttributes.ReadOnly) ? "Да" : "Нет")}");
            Console.WriteLine($"    Скрытый:    {(file.Attributes.HasFlag(FileAttributes.Hidden) ? "Да" : "Нет")}");
            Console.WriteLine($"    Системный:    {(file.Attributes.HasFlag(FileAttributes.System) ? "Да" : "Нет")}");
            Console.WriteLine($"    Архивный:    {(file.Attributes.HasFlag(FileAttributes.Archive) ? "Да" : "Нет")}");
            Console.WriteLine($"    Сжатый:    {(file.Attributes.HasFlag(FileAttributes.Compressed) ? "Да" : "Нет")}");
            Console.WriteLine($"    Зашифрованный:    {(file.Attributes.HasFlag(FileAttributes.Encrypted) ? "Да" : "Нет")}\n");

            Console.WriteLine("ДОСТУП");
        }
        else {
            Console.WriteLine($"Файл {path} не найден");
        }
    }
    static void DisplayFileContent(string path) //Содержимое файла
    {
        FileInfo file = new FileInfo(path);

        if (!file.Exists) 
        {
            Console.WriteLine($"Файл {path} не найден");
            return;
        }
        
        if( file.Length > 5 * 1024 * 1024 )
        {
            Console.WriteLine($"Файл большой ({FormatedFileSize(file.Length)})\nВсе равно прочитать содержимое(+ / -) ?");

            if (Console.ReadLine() != "+")
                return;
        }

        if (IsBinaryFile(path))
        {
            Console.WriteLine($"Ошибка: файл {path} является бинарным");
            return;
        }

        try
        {
            string content = File.ReadAllText(path, Encoding.UTF8);
            Console.WriteLine(content);
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("Нет прав для чтений файла");
        }
        catch (IOException)
        {
            Console.WriteLine($"Ошибка чтения содержимого файла {path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }  
    }
    static void FindFiles(string directory, string pattern) //Поиск файлов в папке по маске
    {
        if (Directory.Exists(directory)) 
        {
            int count = 0;

            foreach (string file in Directory.EnumerateFiles(directory, pattern, SearchOption.AllDirectories))
            {
                Console.WriteLine($"    {file}");
                count++;
            }

            Console.WriteLine($"\n======= Найдено {count} файла/файлов =======");
        }
        else
        {
            Console.WriteLine("Папка не найдена");
        }
    }
    static void FindFiles(string pattern) //Перегрузка метода
    {
        int count = 0;

        foreach (string file in Directory.EnumerateFiles(Directory.GetCurrentDirectory(), pattern, SearchOption.AllDirectories))
        {
            Console.WriteLine($"    {file}");
            count++;
        }

        Console.WriteLine($"\n======= Найдено {count} файла/файлов =======");
    }
    static void CreateZipArchive(string archiveName, string[] files) //Архивация
    {
        if(File.Exists(archiveName))
        {
            Console.WriteLine($"Архив {archiveName} уже существует. Перезаписать (+ / -) ?");

            if (Console.ReadLine() != "+")
                return;
        }

        foreach (string file in files)
        {
            if(!File.Exists(file))
            {
                Console.WriteLine($"Файл {file} не найден");
                return;
            }
        }

        Console.WriteLine($"Создание архива: {Path.GetFileName(archiveName)}");

        using (FileStream zipStream = new FileStream(archiveName, FileMode.Create))
        using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
        {
            int len = files.Length;
            for (int i=0; i < len; ++i)
            {
                string file = files[i];
                FileInfo fileInfo = new FileInfo(file);

                string entryName = Path.GetFileName(file);

                Console.WriteLine($"    Добавляю: {fileInfo.Name} ({FormatedFileSize(fileInfo.Length)})");

                archive.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
            }
        }

        FileInfo zipInfo = new FileInfo(archiveName);

        Console.WriteLine($"    Архив создан: {zipInfo.Name} ({FormatedFileSize(zipInfo.Length)})");
        Console.WriteLine($"    Файлов добавлено: {files.Length}");
    }
    static void ExtractZipArchive(string archivePath, string targetDir = null) //Распаковка архива
    {
        if (!File.Exists(archivePath))
        {
            Console.WriteLine($"Архив {archivePath} не существует");
            return;
        }

        if (targetDir == null)
            targetDir = Directory.GetCurrentDirectory();

        if (!Directory.Exists(targetDir))
            Directory.CreateDirectory(targetDir);

        using (ZipArchive archive = ZipFile.OpenRead(archivePath))
        {
            Console.WriteLine($"    Архив: {Path.GetFileName(archivePath)}");
            Console.WriteLine($"    Файлов: {archive.Entries.Count}");

            foreach (var entry in archive.Entries)
            {
                Console.WriteLine($"   - {entry.FullName} ({FormatedFileSize(entry.Length)})");
            }

            Console.WriteLine($"Распаковать {archive.Entries.Count} файла/файлов в {targetDir} (+ / -) ?");
        }

        if (Console.ReadLine() == "+")
        {
            ZipFile.ExtractToDirectory(archivePath, targetDir);
            Console.WriteLine($"\nРаспаковано в {targetDir}");
        }
        else
            Console.WriteLine("Отмена распаковки");
    }
    static void ChangeDirectory(string current, string newPath) //Смена директории
    { 
        string fullPath = Path.GetFullPath(Path.Combine(current, newPath));

        if(!Directory.Exists(fullPath))
        {
            Console.WriteLine($"Папка {fullPath} не найдена");
        }
        else
        {
            Directory.SetCurrentDirectory(fullPath);
            Console.WriteLine($"Перешёл из {current} в {fullPath}");
        }
    }
    static void CreateDirectory(string path)
    {
        string fullPath = Path.GetFullPath(path);

        if (Directory.Exists(fullPath))
        {
            Console.WriteLine($"Папка уже существует: {Path.GetFileName(path)}");
            Console.WriteLine($"Путь: {fullPath}");
        }
        else
        {
            Directory.CreateDirectory(fullPath);
            Console.WriteLine($"Папка {fullPath} успешно создана");
        }
    }
    static void DisplayHelp()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("=== СПРАВКА ПО КОМАНДАМ ФАЙЛОВОГО МЕНЕДЖЕРА ===");
        Console.ResetColor();

        Console.WriteLine("\nОсновные команды:");
        Console.WriteLine("  ls [путь]                    - вывод содержимого каталога");
        Console.WriteLine("  cp <источник> <приемник>    - копирование файла");
        Console.WriteLine("  mv <источник> <приемник>    - перемещение/переименование");
        Console.WriteLine("  rm <путь>                   - удаление файла");
        Console.WriteLine("  info <путь>                 - подробная информация о файле");
        Console.WriteLine("  cat <путь>                  - вывод содержимого текстового файла");
        Console.WriteLine("  find <каталог> <маска>      - поиск файлов по маске");
        Console.WriteLine("  find <маска>                - поиск в текущем каталоге");
        Console.WriteLine("  zip <архив> <файлы...>      - упаковка файлов в ZIP");
        Console.WriteLine("  unzip <архив> [каталог]     - распаковка архива");
        Console.WriteLine("  cd <путь>                   - смена текущей директории");
        Console.WriteLine("  md <путь>                   - создание директории");
        Console.WriteLine("  help                        - показать эту справку");
        Console.WriteLine("  exit                        - выход из программы");

        Console.WriteLine("\nПримеры использования:");
        Console.WriteLine("  ls .                       - показать содержимое текущей папки");
        Console.WriteLine("  cp file.txt backup/       - скопировать файл в папку backup");
        Console.WriteLine("  mv old.txt new.txt        - переименовать файл");
        Console.WriteLine("  rm temp.txt               - удалить файл");
        Console.WriteLine("  info document.pdf         - показать информацию о файле");
        Console.WriteLine("  cat readme.txt            - прочитать текстовый файл");
        Console.WriteLine("  find . *.txt              - найти все txt файлы");
        Console.WriteLine("  zip backup.zip file1.txt file2.txt");
        Console.WriteLine("  unzip archive.zip         - распаковать в текущую папку");
        Console.WriteLine("  cd ..                     - перейти на уровень выше");
        Console.WriteLine("  md NewFolder              - создать новую папку");

        Console.WriteLine("\nПримечания:");
        Console.WriteLine("  • Пути могут быть абсолютными или относительными");
        Console.WriteLine("  • При копировании/перемещении можно указывать папку как приемник");
        Console.WriteLine("  • Для файлов больше 5MB запрашивается подтверждение чтения");
        Console.WriteLine("  • Бинарные файлы нельзя просмотреть через cat");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nДля получения информации о конкретной команде используйте: help <команда>");
        Console.ResetColor();
    }

    // Дополнительный метод для получения справки по конкретной команде
    static void DisplayCommandHelp(string command)
    {
        command = command.ToLower().Trim();

        Console.WriteLine($"\n=== Справка по команде: {command} ===");

        switch (command)
        {
            case "ls":
                Console.WriteLine("Назначение: Вывод содержимого каталога");
                Console.WriteLine("Синтаксис: ls [путь]");
                Console.WriteLine("Примеры:");
                Console.WriteLine("  ls              - показать текущий каталог");
                Console.WriteLine("  ls C:\\Users     - показать указанный каталог");
                Console.WriteLine("  ls ..           - показать родительский каталог");
                break;

            case "cp":
                Console.WriteLine("Назначение: Копирование файла");
                Console.WriteLine("Синтаксис: cp <источник> <приемник>");
                Console.WriteLine("Примеры:");
                Console.WriteLine("  cp file.txt backup.txt      - создать копию с новым именем");
                Console.WriteLine("  cp data.csv D:\\Backup\\    - скопировать в папку");
                Console.WriteLine("  Примечание: запрашивает подтверждение при перезаписи");
                break;

            case "mv":
                Console.WriteLine("Назначение: Перемещение или переименование файла");
                Console.WriteLine("Синтаксис: mv <источник> <приемник>");
                Console.WriteLine("Примеры:");
                Console.WriteLine("  mv old.txt new.txt          - переименовать файл");
                Console.WriteLine("  mv file.txt ../archive/    - переместить в другую папку");
                break;

            case "rm":
                Console.WriteLine("Назначение: Удаление файла");
                Console.WriteLine("Синтаксис: rm <путь>");
                Console.WriteLine("Примеры:");
                Console.WriteLine("  rm temp.txt                 - удалить файл");
                Console.WriteLine("  Примечание: запрашивает подтверждение удаления");
                break;

            case "info":
                Console.WriteLine("Назначение: Подробная информация о файле");
                Console.WriteLine("Синтаксис: info <путь>");
                Console.WriteLine("Выводит: имя, размер, даты, атрибуты и другую информацию");
                break;

            case "cat":
                Console.WriteLine("Назначение: Просмотр содержимого текстового файла");
                Console.WriteLine("Синтаксис: cat <путь>");
                Console.WriteLine("Примечание: работает только с текстовыми файлами");
                break;

            case "find":
                Console.WriteLine("Назначение: Поиск файлов по маске");
                Console.WriteLine("Синтаксис: find <каталог> <маска>");
                Console.WriteLine("Синтаксис: find <маска> (поиск в текущем каталоге)");
                Console.WriteLine("Примеры:");
                Console.WriteLine("  find . *.txt               - найти txt файлы в текущей папке");
                Console.WriteLine("  find C:\\ *.docx           - найти docx файлы на диске C");
                break;

            case "zip":
                Console.WriteLine("Назначение: Создание ZIP архива");
                Console.WriteLine("Синтаксис: zip <архив> <файлы...>");
                Console.WriteLine("Примеры:");
                Console.WriteLine("  zip backup.zip file1.txt file2.txt");
                Console.WriteLine("  zip photos.zip *.jpg *.png");
                break;

            case "unzip":
                Console.WriteLine("Назначение: Распаковка ZIP архива");
                Console.WriteLine("Синтаксис: unzip <архив> [каталог]");
                Console.WriteLine("Примеры:");
                Console.WriteLine("  unzip archive.zip           - распаковать в текущую папку");
                Console.WriteLine("  unzip backup.zip D:\\Restore - распаковать в указанную папку");
                break;

            case "cd":
                Console.WriteLine("Назначение: Смена текущей директории");
                Console.WriteLine("Синтаксис: cd <путь>");
                Console.WriteLine("Примеры:");
                Console.WriteLine("  cd ..                       - на уровень выше");
                Console.WriteLine("  cd C:\\Program Files        - абсолютный путь");
                Console.WriteLine("  cd Documents                - относительный путь");
                break;

            case "md":
                Console.WriteLine("Назначение: Создание директории");
                Console.WriteLine("Синтаксис: md <путь>");
                Console.WriteLine("Примеры:");
                Console.WriteLine("  md NewFolder               - создать в текущей папке");
                Console.WriteLine("  md C:\\Projects\\New       - создать с указанием пути");
                break;

            default:
                Console.WriteLine("Команда не найдена. Введите 'help' для списка всех команд.");
                break;
        }
    }
    //Вспомогательные методы
    static string FormatedFileSize(long fileSize) //Перевод байт в другие системы измерения
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double size = fileSize;
        int count = 0;

        while (size >= 1024 && count < sizes.Length - 1)
        {
            count++;
            size /= 1024;
        }

        return $"{size:0.##} {sizes[count]}";
    }
    static string GetFileType(string path) //Тип файла
    {
        string extension = Path.GetExtension(path).ToLower();

        return extension switch
        {
            // Текстовые файлы
            ".txt" => "Текстовый файл",
            ".log" => "Файл лога",
            ".csv" => "CSV таблица",
            ".xml" => "XML документ",
            ".json" => "JSON файл",
            ".html" or ".htm" => "Веб-страница",

            // Документы
            ".pdf" => "PDF документ",
            ".doc" or ".docx" => "Документ Word",
            ".xls" or ".xlsx" => "Таблица Excel",
            ".ppt" or ".pptx" => "Презентация PowerPoint",

            // Изображения
            ".jpg" or ".jpeg" => "Изображение JPEG",
            ".png" => "Изображение PNG",
            ".gif" => "Анимированное изображение GIF",
            ".bmp" => "Изображение BMP",
            ".svg" => "Векторное изображение SVG",

            // Архивы
            ".zip" => "Архив ZIP",
            ".rar" => "Архив RAR",
            ".7z" => "Архив 7-Zip",
            ".tar" => "Архив TAR",
            ".gz" => "Сжатый архив GZIP",

            // Исполняемые файлы
            ".exe" => "Исполняемый файл Windows",
            ".msi" => "Установщик Windows",
            ".bat" or ".cmd" => "Пакетный файл",
            ".sh" => "Скрипт Shell",

            // Код
            ".cs" => "Исходный код C#",
            ".java" => "Исходный код Java",
            ".cpp" or ".c" => "Исходный код C/C++",
            ".py" => "Скрипт Python",
            ".js" => "Скрипт JavaScript",
            ".css" => "Таблица стилей CSS",

            // Видео/Аудио
            ".mp4" => "Видео MP4",
            ".avi" => "Видео AVI",
            ".mp3" => "Аудио MP3",
            ".wav" => "Аудио WAV",

            // Конфигурационные
            ".config" => "Файл конфигурации",
            ".ini" => "INI файл настроек",
            ".yml" or ".yaml" => "YAML конфигурация",

            // Прочее
            ".dll" => "Библиотека DLL",
            ".lnk" => "Ярлык Windows",
            ".iso" => "Образ диска",

            _ => "Файл" // По умолчанию
        };
    }
    static bool AskOverWrite(FileInfo sourceFile, string destination) // Метод для подтверждения
    {
        Console.WriteLine($"Файл '{Path.GetFileName(destination)}' уже существует");
        Console.WriteLine("Перезаписать (+ / -)?");

        string command = Console.ReadLine().Trim();

        if (command == "+") return true;
        return false;
    }
    static bool IsBinaryFile(string path) //Бинарный ли файл
    {
        byte[] buffer = new byte[1024]; //Читаем первые 1 KB и ищем нулевые байты
        using (var stream = File.OpenRead(path))
        {
            int bytes = stream.Read(buffer, 0, buffer.Length);
            return buffer.Take(bytes).Any(b => b == 0);
        }
    }
}