using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rtdb_to_sqlite_database.classes;

namespace rtdb_to_sqlite_database
{
    internal class Program
    {
        #region Структура БД ассамблеи
        /* 
        [STAT_VSP_CONF] - статистика конференции
            SEANCE_NO=int
            CLUSTER_ID=int
            SCHEME_ID=int
            START_TYPE_ID=int
            DT_BEGIN=timestamp (ex. - 1671868800)
            DT_END=timestamp (ex. - 1671877734)
            DT_COLLECTION=timestamp (ex. - 1671868822)
            STARTUSER_ID#
            STARTUSER_NAME#
            IDENTIFY_CODE=int
            FULL_DURATION=int
            WORK_DURATION=int
            CLUSTER_NAME=Новый кластер
            SCHEME_NAME=string (ex. - Название селектора)
            CONTRACT_NO=
            RESERVED_CH=int
            RESERVED_DSP=int
            RESERVED_TIME=int
            IS_PLANCONF=int
            FINISH_REASON=int
            FINISH_USER_ID#
            FINISH_USER_NAME# 
        */

        /*
        [STAT_VSP_CONF_PLAN] - планировщик
            CLUSTER_ID=int
            ID=int
            MODIFY_NO=int
            DT_ACTION=timestamp (ex. - 1671953100)
            DT_BEGIN=timestamp (ex. - 1671953100)
            DT_END=timestamp (ex. - 1671954600)
            DSP_RES_ID=int
            CH_CNT=int
            DSPRES_CNT=int
            SCHEME_ID=int
            AUTO_START=int
            SERIA_ID=int
            IS_MODIFIED_IN_SERIA=int
            CLUSTER_NAME=Новый кластер
            SCHEME_NAME=string (ex. - Название селектора)
            ACTION_TYPE=int
            TASK_ID=int
            USER_LOGIN#
            STAT_SEANCE_NO = int
            STAT_RESULT=int
            ACTUAL_CH_CNT=int
            ACTUAL_DSPRES_CNT=int
        */
        /*
        [VSP_CONF_SCHEMES]
            CLUSTER_ID=int
            ID=int
            NAME=string (ex. - Название селектора)
            IDENTIFY_CODE =int
            NP_MAX_CH=int
            NP_MAX_DSP=int
            NP_MAX_TIME=int
        */
        #endregion


        static Dictionary<string, List<string>> settings_dictionary = new Dictionary<string, List<string>> { };
        static Dictionary<int, VSP_CONF_SCHEMES> conferences = new Dictionary<int, VSP_CONF_SCHEMES> { };
        static Dictionary<string, List<List<string>>> data_base_dictionary = new Dictionary<string, List<List<string>>> { };

      //  static string data_base_filename = "";
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

             //   data_base_filename = Path.GetFileName(settings_dictionary["data_base"].First());

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


        private static void load_rtdb_to_Dictionary()
        {
            string[] data_rtdb = File.ReadAllLines(Path.GetFullPath(settings_dictionary["data_base"].First()));
            string last_key = "";
            List<string> data = new List<string>(); // значения для одной записи
            foreach (string db_string in data_rtdb)
            {
                if (db_string != "") // записи в БД разделены пустой строкой
                {
                    if (db_string[0] == '[') // название таблицы в квадратных скобках
                    {
                        last_key = db_string.Split(']')[0].Replace('[', ' ').Trim(); // запоминаем название таблицы и делаем его ключом в словаре
                        if (!data_base_dictionary.ContainsKey(last_key)) // если такого ключа нет, добавлем ключ и пустой список значений
                        {
                            data_base_dictionary.Add(last_key, new List<List<string>> { });
                        }
                    }
                    else
                    {
                        data.Add(db_string.Trim());
                        //      data_base_dictionary[last_key].Add(db_string.Trim());
                    }
                }
                else
                {
                    data_base_dictionary[last_key].Add(data);
                    data = new List<string>();
                }
            }


        }

        public static void VSP_CONF_SCHEMES_loader()
        {
            foreach (List<string> item in data_base_dictionary["VSP_CONF_SCHEMES"])
            {
                VSP_CONF_SCHEMES conf = new VSP_CONF_SCHEMES(item);
                if (!conferences.ContainsKey(conf.ID))
                {
                    conferences.Add(conf.ID, conf);
                }
                else
                {
                    add_to_main_log("в словаре конференций одинаковые ID: [" + conf.ID.ToString() + " " + conf.Name + "] и [" + conferences[conf.ID].ID.ToString() + " " + conferences[conf.ID].Name);
                }
            }
        }


    }
}
