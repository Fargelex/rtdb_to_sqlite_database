using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rtdb_to_sqlite_database
{
    internal class Program
    {
        static Dictionary<string, List<string>> settings_dictionary = new Dictionary<string, List<string>> { };
        static string data_base_filename = "";
        static void Main(string[] args)
        {
            load_settings();
            Console.ReadKey();
        }


        private static void add_to_main_log(string text_log, bool add_to_text_log = true)
        {
            string log_string = '[' + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "]\t" + text_log.Trim();
            Console.WriteLine(log_string);
            if (add_to_text_log)
            {
                File.AppendAllText("rtdb_to_sqlite_database.log", log_string + "\r\n");
            }
        }

        private static bool load_settings()
        {
            bool no_errors = true;
            string file_name_ini = "rtdb_to_sqlite_database.ini";

            if (File.Exists(file_name_ini))
            {
                string[] settings_file_lines = File.ReadAllLines(file_name_ini);
                string last_key = "";
                foreach (string settings_file_line in settings_file_lines)
                {
                    if (settings_file_line[0] == '[')
                    {
                        last_key = settings_file_line.Split(']')[0].Replace('[', ' ').Trim();
                        if (!settings_dictionary.ContainsKey(last_key))
                        {
                            settings_dictionary.Add(last_key, new List<string> { });
                        }
                    }
                    else
                    {
                        settings_dictionary[last_key].Add(settings_file_line.Trim());
                    }
                }

                if (!File.Exists(settings_dictionary["data_base"].First()))
                {
                    add_to_main_log("в файле ["+ file_name_ini + "] указан путь [" + Path.GetFullPath(settings_dictionary["data_base"].First()) + "] в котором отсутствует файл базы данных - data.rtdb");
                    // add_to_main_log(Path.GetFullPath(settings_dictionary["data_base"].First()), false);
                    no_errors = false;
                }

                data_base_filename = Path.GetFileName(settings_dictionary["data_base"].First());

            }
            else
            {
                //  add_to_main_log("info\tфайл с настройками settings.ini отсутствует, создаю файл по умолчанию");
                string[] default_settings = new string[] {
                "[data_base] путь к базе данных",
                    @"db\data.rtdb"
                };
                File.WriteAllLines(file_name_ini, default_settings);
                load_settings();
            }
            return no_errors;
        }


    }
}
