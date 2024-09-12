using System;
using System.Text;
using System.Transactions;

namespace CLOPE_Algorithm
{
    class Program
    {
        static void Main(string[] args)
        {
            const string path = @"./agaricus-lepiota.data";
            const double r = 2.6;

            var clope = new CLOPE(r);

            // Читаем файл с транзакциями.
            using (StreamReader _sw = new StreamReader(path, Encoding.Default))
            {
                while (!_sw.EndOfStream)
                {
                    List<string> strs = new List<string>();
                    string[] instr = _sw.ReadLine().Split(",");
                    // Добавление к меткам их номера для уникальности
                    for (int i = 0; i < instr.Length; i++)
                    {
                        if (instr[i] != "?")
                        {
                            strs.Add(instr[i] + i);
                        }
                    }
                    clope.EAT(strs.ToArray());
                }
                _sw.Close();
            }
            Console.WriteLine($"Кластеров после инициалиазиции - {clope.clusters.Count}");

            // Вывод в консоль кластеров, сьедобных и несъедобных грибов
            for (int i = 0; i < clope.clusters.Count; i++)
            {
                int e = 0;
                int p = 0;
                foreach (var key in clope.transactions.Keys)
                {
                    if (clope.transactions[key] == clope.clusters[i].id)
                    {
                        if (key[0] == "e0")
                        {
                            e++;
                        }
                        else
                        {
                            p++;
                        }
                    }
                }
                Console.WriteLine($"Кластер {i + 1} - {e} / {p}");
            }

            clope.Iterate(5);

            Console.WriteLine($"Кластеров после итерации - {clope.clusters.Count}");

            // Вывод в консоль кластеров, сьедобных и несъедобных грибов
            for (int i = 0; i < clope.clusters.Count; i++)
            {
                int e = 0;
                int p = 0;
                foreach (var key in clope.transactions.Keys)
                {
                    if (clope.transactions[key] == clope.clusters[i].id)
                    {
                        if (key[0] == "e0")
                        {
                            e++;
                        }
                        else
                        {
                            p++;
                        }
                    }
                }
                Console.WriteLine($"Кластер {i + 1} - {e} / {p}");
            }

        }
    }
}
