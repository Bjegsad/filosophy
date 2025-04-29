using System;
using System.Threading;
using System.Xml;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Let's go");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("filosophy.xml"); //Считывание из XML

            //Парсинг параметров из XML
            int N = int.Parse(xmlDoc.SelectSingleNode("filosophers/filosophers_count").InnerText);
            Console.WriteLine(N);
            bool dbg = bool.Parse(xmlDoc.SelectSingleNode("filosophers/debug_mode").InnerText);
            int duration = int.Parse(xmlDoc.SelectSingleNode("filosophers/duration_ms").InnerText);

            //Создание массива вилок
            Fork[] forks = new Fork[N];
            for (int i = 0; i < N; i++)
            {
                forks[i] = new Fork();
            }

            // Создание массива философов
            Filosopher[] filosophers = new Filosopher[N];
            for (int i = 0; i < N; i++)
            {
                // Каждый философ получает левую и правую вилки
                filosophers[i] = new Filosopher(i + 1, forks[(i + 1) % N], forks[i], dbg);
            }

            // Создание и запуск потоков для каждого философа
            Thread[] runners = new Thread[N];
            for (int i = 0; i < N; i++)
            {
                runners[i] = new Thread(filosophers[i].run);
            }
            for (int i = 0; i < N; i++)
            {
                runners[i].Start();
            }

            // Ожидание указанное в конфигурации время
            Thread.Sleep(duration);

            // Остановка всех философов
            for (int i = 0; i < N; i++)
            {
                filosophers[i].stop();
            }

            // Ожидание завершения всех потоков
            for (int i = 0; i < N; i++)
            {
                runners[i].Join();
            }

            // Вывод статистики по каждому философу
            for (int i = 0; i < N; i++)
            {
                filosophers[i].printStats();
            }
        }
    }

    // Класс, представляющий вилку (разделяемый ресурс)
    class Fork
    {
        private Mutex m = new Mutex(); // Мьютекс для синхронизации доступа к вилке

        // Метод для взятия вилки (захват мьютекса)
        public void take()
        {
            m.WaitOne();
        }

        // Метод для освобождения вилки (освобождение мьютекса)
        public void put()
        {
            m.ReleaseMutex();
        }
    };

    // Класс, представляющий философа
    class Filosopher
    {
        int id;                // Идентификатор философа
        Fork fork_left;        // Левая вилка
        Fork fork_right;       // Правая вилка
        uint eat_count;        // Счетчик приемов пищи
        double wait_time;     // Общее время ожидания (голода)
        DateTime wait_start;   // Время начала ожидания
        bool stop_flag;        // Флаг для остановки потока
        bool debug_flag;       // Флаг для вывода отладочной информации
        Random random;         // Генератор случайных чисел

        // Конструктор класса Philosopher
        public Filosopher(int number, Fork left, Fork right, bool dbg)
        {
            this.id = number;
            this.fork_left = left;
            this.fork_right = right;
            this.eat_count = 0;
            this.wait_time = 0;
            this.debug_flag = dbg;
            this.stop_flag = false;
            this.random = new Random();
        }

        // Основной метод работы философа
        public void run()
        {
            // Бесконечный цикл, пока не установлен stop_flag
            while (!stop_flag)
            {
                think();

                // Взятие левой вилки
                this.fork_left.take();
                if (this.debug_flag)
                {
                    Console.WriteLine(this.id + " took left fork");
                }

                // Взятие правой вилки
                this.fork_right.take();
                if (this.debug_flag)
                {
                    Console.WriteLine(this.id + " took right fork");
                }

                eat();

                // Освобождение правой вилки
                this.fork_right.put();
                if (this.debug_flag)
                {
                    Console.WriteLine(this.id + " put right fork");
                }

                // Освобождение левой вилки
                this.fork_left.put();
                if (this.debug_flag)
                {
                    Console.WriteLine(this.id + " put left fork");
                }
            }
        }

        // Метод, имитирующий размышления философа
        void think()
        {
            if (this.debug_flag)
            {
                Console.WriteLine(this.id + " thinking");
            }

            // Случайная задержка для размышлений
            Thread.Sleep(this.random.Next(0, 100));

            if (this.debug_flag)
            {
                Console.WriteLine(this.id + " hungry");
            }

            // Засекаем время начала ожидания вилок
            this.wait_start = DateTime.Now;
        }

        // Метод, имитирующий прием пищи
        void eat()
        {
            // Добавляем время ожидания вилок
            this.wait_time += DateTime.Now.Subtract(this.wait_start).TotalMilliseconds;
            if (this.debug_flag)
            {
                Console.WriteLine(this.id + " eating");
            }

            // Случайная задержка для приема пищи
            Thread.Sleep(this.random.Next(0, 100));

            // Увеличиваем счетчик приемов пищи
            eat_count++;
        }

        // Метод для остановки работы философа
        public void stop()
        {
            stop_flag = true;
        }

        // Метод для вывода статистики
        public void printStats()
        {
            Console.WriteLine("философ" + "\t" + "кол-во приемов пищи" + "\t" + "Время голода");
            Console.WriteLine(this.id + "\t" + this.eat_count + "\t" + Convert.ToInt32(this.wait_time));
        }
    }
}
