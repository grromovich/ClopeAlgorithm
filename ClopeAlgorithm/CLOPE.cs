using System;

namespace CLOPE_Algorithm
{
    class CLOPE
    {
        public class Cluster
        {
            public int id { get; set; }
            public int N { get; set; } = 0;
            public Dictionary<string, int> Width { get; set; } = new Dictionary<string, int>();
            public int Square { get; set; } = 0;
        }

        public List<Cluster> clusters = new List<Cluster>();
        public Dictionary<string[], int> transactions = new Dictionary<string[], int>();
        public double r = 1.1;

        public CLOPE(double r)
        {
            this.r = r;
        }

        // Инициализация по одного элемента
        public bool EAT(string[] InStr)
        {
            if (transactions.ContainsKey(InStr))
            {
                return false;
            }
            else
            {
                // Первая транзация попадает в первый кластер
                if (clusters.Count == 0)
                {
                    clusters.Add(new Cluster { id = 0 });
                    AddToCluster(0, InStr);
                    transactions.Add(InStr, 0);
                }
                else
                {
                    double maxValue = 0;
                    int clusterMaxInt = 0;
                    // Перебор всех класетров для вычесления максимальной стоимости
                    for (int i = 0; i < clusters.Count; i++)
                    {
                        double cost = AddCost(clusters[i], InStr);
                        if (cost > maxValue)
                        {
                            maxValue = cost;
                            clusterMaxInt = clusters[i].id;
                        }
                    }
                    // Стоимость добавления в пустой кластер
                    double empty = AddCost(InStr);
                    // Добавление в кластер с наибольшей стоимостью
                    if (empty > maxValue)
                    {
                        int newId = clusters.Last().id + 1;
                        clusters.Add(new Cluster { id = newId });
                        AddToCluster(newId, InStr);
                        transactions.Add(InStr, newId);
                    }
                    else
                    {
                        AddToCluster(clusterMaxInt, InStr);
                        transactions.Add(InStr, clusterMaxInt);
                    }
                }
            }
            return true;
        }

        public void Iterate(int maxSteps)
        {
            int steps = 0;
            bool moved = true;
            while (moved && steps < maxSteps)
            {
                steps++;

                int countNewClusters = 0;
                int countMovesElement = 0;

                moved = false;
                // Перебор всех транзакций с вычислением стоимостью перемещения
                foreach (var key in transactions.Keys)
                {
                    double removeCost = RemoveCost(transactions[key], key);
                    double maxRemoveValue = removeCost;
                    int maxRemoveId = 0;
                    // Перебор всех кластеров для вычисления максимальной стоимости
                    for (int i = 0; i < clusters.Count; i++)
                    {
                        if (clusters[i].id == transactions[key])
                        {
                            continue;
                        }
                        double cost = AddCost(clusters[i], key);

                        if (cost > maxRemoveValue && (cost + removeCost) > 0)
                        {
                            maxRemoveValue = cost;
                            maxRemoveId = clusters[i].id;
                        }
                    }
                    
                    // Стоимость добавления в пустой кластер
                    double empty = AddCost(key);
                    
                    // Добалвение в кластер с максимальной стоимостью перемещения
                    if (empty > maxRemoveValue && (empty + removeCost) > 0)
                    {
                        moved = true;
                        int newId = clusters.Last().id + 1;

                        RemoveFromCluster(transactions[key], key);
                        clusters.Add(new Cluster { id = newId });
                        AddToCluster(newId, key);
                        transactions[key] = newId;

                        countNewClusters++;
                    }
                    else if (maxRemoveValue > removeCost)
                    {
                        moved = true;

                        RemoveFromCluster(transactions[key], key);
                        AddToCluster(maxRemoveId, key);
                        transactions[key] = maxRemoveId;

                        countMovesElement++;
                    }
                }
                Console.WriteLine($"Переставлено {countMovesElement} элементов и {countMovesElement} новых кластеров");
            }
            // Удаление всех пустых кластеров
            for (int i = 0; i < clusters.Count; i++)
            {
                if (clusters[i].N < 1)
                {
                    clusters.Remove(clusters[i]);
                }
            }
        }


        // Функция вычисления стоимости добавления
        private double AddCost(Cluster cluster, string[] transaction)
        {
            int widthOld = cluster.Width.Keys.Count;
            int squareOld = cluster.Square;

            int widthNew = GetNewWidth(cluster, transaction);
            int squareNew = cluster.Square + transaction.Length;

            double oldCost = squareOld * cluster.N / Math.Pow(widthOld, r);
            double newCost = squareNew * (cluster.N + 1) / Math.Pow(widthNew, r);

            return newCost - oldCost;
        }

        // Функция вычисления стоимости добавления в пустой кластер
        private double AddCost(string[] transaction)
        {
            double newCost = transaction.Length / Math.Pow(transaction.Length, r);

            return newCost;
        }

        // Функция вычисления стоимости удаления
        private double RemoveCost(int clusterId, string[] transaction)
        {
            var cluster = clusters.First(c => c.id == clusterId);

            int widthOld = cluster.Width.Keys.Count;
            int squareOld = cluster.Square;

            int widthNew = GetNewWidthWithoutTransaction(cluster, transaction);
            int squareNew = cluster.Square - transaction.Length;

            double oldCost = squareOld * cluster.N / Math.Pow(widthOld, r);
            double newCost = squareNew * (cluster.N - 1) / Math.Pow(widthNew, r);

            return newCost - oldCost;
        }


        // Добавление транзакции в кластер
        private void AddToCluster(int clusterId, string[] transaction)
        {
            var cluster = clusters.First(c => c.id == clusterId);

            cluster.N++;
            cluster.Square += transaction.Length;
            // Добавление меток транзации в кластер
            for (int i = 0; i < transaction.Length; i++)
            {
                if (cluster.Width.ContainsKey(transaction[i]))
                {
                    cluster.Width[transaction[i]]++;
                }
                else
                {
                    cluster.Width.Add(transaction[i], 1);
                }
            }
        }

        // Удаление транзации из кластера
        private void RemoveFromCluster(int clusterId, string[] transaction)
        {
            var cluster = clusters.First(c => c.id == clusterId);

            cluster.N--;
            cluster.Square -= transaction.Length;
            // Удаление меток транзации из кластера
            for (int i = 0; i < transaction.Length; i++)
            {
                cluster.Width[transaction[i]]--;
                if (cluster.Width[transaction[i]] == 0)
                {
                    cluster.Width.Remove(transaction[i]);
                }
            }
        }

        // Функция подсчета новой ширины транзации и кластера
        private int GetNewWidth(Cluster cluster, string[] transaction)
        {
            // Копирование меток в новый словарь
            var oldWidth = new Dictionary<string, int>();

            foreach (var key in cluster.Width.Keys)
            {
                oldWidth.Add(key, cluster.Width[key]);
            }

            // Добавление меток транзации к новому словарю
            for (int i = 0; i < transaction.Length; i++)
            {
                if (oldWidth.ContainsKey(transaction[i]))
                {
                    oldWidth[transaction[i]]++;
                }
                else
                {
                    oldWidth.Add(transaction[i], 1);
                }
            }
            return oldWidth.Keys.Count;
        }

        // Функция подсчета новой ширины кластера без транзакции
        private int GetNewWidthWithoutTransaction(Cluster cluster, string[] transaction)
        {
            // Копирование меток в новый словарь
            var oldWidth = new Dictionary<string, int>();

            foreach (var key in cluster.Width.Keys)
            {
                oldWidth.Add(key, cluster.Width[key]);
            }
            // Удаление меток транзации из нового словаря
            for (int i = 0; i < transaction.Length; i++)
            {
                oldWidth[transaction[i]]--;
                if (oldWidth[transaction[i]] == 0)
                {
                    oldWidth.Remove(transaction[i]);
                }
            }
            return oldWidth.Keys.Count;
        }
    }
}
