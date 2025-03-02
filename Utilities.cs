using System.Text.Json;
using System.Text.Json.Serialization;

namespace ContactBook
{
    public static class Utilities
    {
        /// <summary>
        /// Imprime un string por consola con un margin a la izquierda.
        /// </summary>
        public static void Print(string text)
        {
            Console.CursorLeft = 2;
            foreach (string line in text.Split('\n'))
            {
                Console.Write(line);
                Console.SetCursorPosition(2, Console.CursorTop + 1);
            }
            Console.CursorLeft = 0;
        }

        /// <summary>
        /// Imprime un título con formato preestablecido en consola.
        /// </summary>
        public static void WriteTitle(string title)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine('\t' + title + '\n');
            Console.ResetColor();
        }

        /// <summary>
        /// Muestra un mensaje de alerta predeterminado.
        /// </summary>
        /// <param name="msg">Mensaje a mostrar.</param>
        public static void ShowAlert(string msg)
        {
            (int cX, int cY) = Console.GetCursorPosition();
            Console.SetCursorPosition(0, Console.WindowHeight - 3);
            Print(msg + new string(' ', Console.WindowWidth - msg.Length));
            Console.SetCursorPosition(cX, cY);
        }

        /// <summary>
        /// Muestra un mensaje de alerta que se oculta luego del tiempo indicado.
        /// </summary>
        /// <param name="msg">Mensaje a mostrar.</param>
        /// <param name="danger">Booleano que indica si el mensaje es de peligro o no.</param>
        /// <param name="secs">Tiempo de visualización de la alerta.</param>
        public static void ShowAlert(string msg, bool danger, float secs = 3)
        {
            Console.ForegroundColor = danger ? ConsoleColor.DarkRed : ConsoleColor.DarkCyan;
            ShowAlert(msg);
            Thread.Sleep((int)(secs * 1000));
            ClearAlert();
        }

        /// <summary>
        /// Oculta el mensaje de alerta.
        /// </summary>
        public static void ClearAlert()
        {
            Console.ResetColor();
            ShowAlert("");
        }

        /// <summary>
        /// Muestra un menú de atajos de teclado en la consola y permite reaccionar con acciones preestablecidas según 
        /// la opción seleccionada por el usuario.
        /// </summary>
        /// <param name="shortcuts">Lista de atajos a mostrar.</param>
        /// <param name="actions">Un diccionario basado en clave - acción, donde cada clave es un entero que 
        /// representa el código ascii del caracter seleccionado.</param>
        public static void ShortcutsMenu(string[,] shortcuts)
        {
            int length = shortcuts.GetUpperBound(0) + 1, rows = 1;
            Console.SetCursorPosition(0, Console.WindowHeight - 2);
            for (int i = 0; i < 2; i++) Console.Write(new string(' ', Console.WindowWidth));
            while (rows * 4 < length) rows++;
            Console.SetCursorPosition(2, Console.WindowHeight - rows);
            for (int i = 0; i < length; i++)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write('[' + shortcuts[i, 0] + ']');
                Console.ResetColor();
                Console.Write(" " + shortcuts[i, 1] + "\t");
                if (i != length - 1 && (i + 1) % 4 == 0) Console.Write("\n  ");
            }
        }

        /// <summary>
        /// Agrega un caracter a un campo y lo imprime en consola.
        /// </summary>
        /// <param name="c">El caracter a escribir en el campo.</param>
        /// <param name="field">Hace referencia al valor del campo en cuestión.</param>
        public static void WriteInField(char c, ref string field)
        {
            if (c == 9) return;
            field += c;
            Console.CursorLeft--;
            Console.Write(c + "█");
        }

        /// <summary>
        /// Borra el último caracter de un campo y actualiza en consola.
        /// </summary>
        /// <param name="field">Hace referencia al valor del campo en cuestión.</param>
        public static void EraseInField(ref string field)
        {
            if (field.Length == 0) return;
            field = field.Substring(0, field.Length - 1);
            Console.CursorLeft -= 2;
            Console.Write("█ ");
            Console.CursorLeft--;
        }

        public static void Sort<T>(ref List<T> data, Func<T, T, int> comparer)
        {
            if (data.Count <= 1) return;

            int r;
            List<T> left = [], equals = [], right = [];

            for (int i = 0; i < data.Count; i++)
            {

                r = comparer.Invoke(data[i], data[0]); // data[0] as pivot.
                if (r == 0) equals.Add(data[i]);
                else if (r > 0) right.Add(data[i]);
                else left.Add(data[i]);
            }

            Sort(ref left, comparer);
            Sort(ref right, comparer);
            data = left.Concat(equals).Concat(right).ToList();
        }

        public static List<T> LoadData<T>(string dbPath)
        {
            List<T> data = JsonSerializer.Deserialize<List<T>>(File.ReadAllText(dbPath));
            if (data == null) data = new List<T>();
            return data;
        }

        /// <summary>
        /// Show a dialog message and returns a value that indicates if accept or denegate the action.
        /// </summary>
        /// <param name="question">The alert to show.</param>
        /// <param name="isDangerous">Indicates if alert is critical or no.</param>
        /// <returns></returns>
        public static bool ShowDialog(string question, bool isDangerous = false)
        {
            string[,] shortcuts =
            {
                { "s", "Si" },
                { "esc", "No" }
            };

            Console.ForegroundColor = isDangerous ? ConsoleColor.Red : ConsoleColor.White;
            ShowAlert(question);
            if (isDangerous) Console.ResetColor();
            ShortcutsMenu(shortcuts);

            bool answer = Console.ReadKey(true).Key == ConsoleKey.S;
            ClearAlert();
            return answer;
        }
    }
}
