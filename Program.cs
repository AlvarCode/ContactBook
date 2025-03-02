namespace ContactBook
{
    class Program
    {
        static List<Contact> contacts = new List<Contact>();
        static int scroll = 0;
        static int maxLength = 0;
        static int selected = -1;
        static List<Contact>? deletedContacts;

        static int MaxContactCount
        {
            get { return Console.WindowHeight - 8; }
        }

        static int CurrentContact
        {
            get { return selected + scroll * MaxContactCount; }
        }

        // static void Main()
        // {
        //     List<Contact> data = Utilities.LoadData<Contact>("./DB.json");
        //     data = Utilities.Sort(data, (a, b) => a.Name[0] - b.Name[0]);
        //     data.ForEach(c => Console.Write(c.Name + ", "));
        // }

        static void Main()
        {
            const string S = "    ";
            ConsoleKeyInfo kCode;
            string[,] shortcuts =
            {
                { "↑", "Subir" },
                { "↓", "Bajar" },
                { "↲", "Info" },
                { "n", "Nuevo" },
                { "b", "Buscar" },
                { "e", "Eliminar" },
                { "sft+e", "Vaciar" },
                { "esc", "Salir" }
            };

            contacts = Utilities.LoadData<Contact>("./DB.json");
            SortContacts();

            while (true)
            {
                Console.CursorVisible = false;

                // contact load
                Console.ResetColor();
                Utilities.WriteTitle("AGENDA DE CONTACTOS");
                Utilities.ShortcutsMenu(shortcuts);
                LoadContactList();
                Console.SetCursorPosition(0, Console.WindowHeight - 4);
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write(new string('-', maxLength + 4));

                while (true)
                {
                    kCode = Console.ReadKey(true);

                    if (kCode.Key == ConsoleKey.UpArrow) Previus();
                    else if (kCode.Key == ConsoleKey.DownArrow) Next();
                    else if (kCode.Key == ConsoleKey.Enter && ContactInfo()) break;
                    else if (kCode.Key == ConsoleKey.N)
                    {
                        int aux = selected;
                        selected = -1;
                        ContactInfo(["", "", "", ""]);
                        selected = aux;
                        break;
                    }
                    else if (kCode.Key == ConsoleKey.B)
                    {
                        SearchContact();
                        Utilities.ClearAlert();
                        Utilities.ShortcutsMenu(shortcuts);
                    }
                    else if (kCode.KeyChar == 101)      // e
                    {
                        bool deleted = DeleteContact();
                        Utilities.ShortcutsMenu(shortcuts);
                        if (deleted) Utilities.ShowAlert("Contacto eliminado correctamente", false);
                    }
                    else if (kCode.KeyChar == 69)    // shift + e
                    {
                        int count = contacts.Count();
                        bool cleaned = ClearContacts();
                        Utilities.ShortcutsMenu(shortcuts);
                        if (cleaned) Utilities.ShowAlert($"Se eliminaron {count} contactos", false);
                    }
                    else if (kCode.Key == ConsoleKey.Escape)
                    {
                        if (Close()) return;
                        Console.SetCursorPosition(0, Console.WindowHeight - 2);
                        Console.Write(new string(' ', Console.WindowWidth - 1));
                        Utilities.ShortcutsMenu(shortcuts);
                    }
                }
            }
        }

        static void LoadContactList(bool isFound = false)
        {
            Console.SetCursorPosition(2, 2);
            Console.Write((isFound ? "Encontrados: " : "Mis contactos: ") + $"({contacts.Count})");
            Console.Write(new string(' ', Console.WindowWidth - Console.CursorLeft));

            int limit = contacts.Count - scroll * MaxContactCount;
            string list = "";
            if (MaxContactCount < limit) limit = MaxContactCount;
            for (int i = 0; i < limit; i++) list += "  " + Row(i + MaxContactCount * scroll) + '\n';
            for (int i = limit; i < MaxContactCount; i++) list += new string(' ', maxLength + 2) + '\n'; // Cleaning
            Console.SetCursorPosition(0, 4);
            Console.Write(list);
            if (selected > -1 && selected < MaxContactCount) SelectRow();
        }

        static string Row(int idx)
        {
            string row = $" {idx + 1}. {contacts[idx].Name} - {contacts[idx].Phone} ";
            if (row.Length > maxLength) maxLength = row.Length;
            else row += new string(' ', maxLength - row.Length);
            return row;
        }

        static void SelectRow()
        {
            Console.SetCursorPosition(0, 4 + selected);
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
            Utilities.Print(Row(CurrentContact));
            Console.ResetColor();
        }

        static void Previus()
        {
            if (CurrentContact <= 0) return;

            if (selected <= 0)
            {
                scroll--;
                selected = MaxContactCount;
                LoadContactList();
            }
            UpdateSelection(-1);
        }

        static void Next()
        {
            if (CurrentContact + 1 == contacts.Count) return;

            if (selected == MaxContactCount - 1)
            {
                scroll++;
                selected = -1;
                LoadContactList();
            }
            UpdateSelection(1);
        }

        static void UpdateSelection(int dir)
        {
            if (selected > -1 && selected < MaxContactCount)
            {
                Console.SetCursorPosition(2, 4 + selected);
                Console.ResetColor();
                Console.Write(Row(CurrentContact));
            }

            selected += 1 * dir;
            SelectRow();
        }
        
        /// <summary>
        /// Valida si se puede mostrar información del contacto y retorna un valor que indica si tuvo exito.
        /// </summary>
        static bool ContactInfo()
        {
            if (contacts.Count == 0) return false;
            if (selected == -1)
            {
                Utilities.ShowAlert("Seleccione el contacto a mostrar", true);
                return false;
            }
            ContactInfo(contacts[CurrentContact]);
            return true;
        }

        static void ContactInfo(Contact contact)
        {
            ContactInfo([contact.Name, contact.Phone, contact.Email, contact.Address]);
        }

        static void ContactInfo(string[] data)
        {
            int i = 0;
            string template = "";

            template += $"Nombre     :  {data[0]}\n";
            template += $"Teléfono   :  {data[1]}\n";
            template += $"Correo     :  {data[2]}\n";
            template += $"Dirección  :  {data[3]}";

            Action<sbyte> focusField = (sbyte s) =>
            {
                if (s == -1 && i == 0) return;
                else if (s == 1 && i == data.Length - 1) return;

                Console.SetCursorPosition(16 + data[i].Length, i + 2);
                Console.Write(' ');
                i += 1 * s;
                Console.SetCursorPosition(16 + data[i].Length, i + 2);
                Console.Write('█');
            };

            bool readOnly = selected != -1;
            Action enableEditionMode = () =>
            {
                string[,] shortcuts =
                {
                    { "↑", "Anterior" },
                    { "↓/↲", "Siguiente" },
                    { selected == -1 ? "!g" : "!g", selected == -1 ? "Crear" : "Guardar" },
                    { "esc", "Cancelar" }
                };
                Utilities.ShortcutsMenu(shortcuts);
                Console.SetCursorPosition(16 + data[i].Length, 2 + i);
                Console.Write('█');
                readOnly = false;
            };
            Action disableEditionMode = () =>
            {
                string[,] shortcuts =
                {
                    { "e", "Editar" },
                    { "esc", "Regresar" }
                };

                if (Console.CursorLeft > 0)
                {
                    Console.CursorLeft--;
                    Console.Write(' ');
                    Console.CursorLeft--;
                }

                Utilities.ShortcutsMenu(shortcuts);
                readOnly = true;
            };

            // ========= Printing ==========
            Utilities.WriteTitle("Información de Contacto");
            Utilities.Print(template);
            (readOnly ? disableEditionMode : enableEditionMode)();

            bool blocked = false;
            ConsoleKeyInfo kCode;

            while (true)
            {
                kCode = Console.ReadKey(true);

                // Visualization mode
                if (readOnly)
                {
                    if (kCode.KeyChar == 69 || kCode.KeyChar == 101) enableEditionMode();   //eE
                    else if (kCode.KeyChar == 27 && SaveContactInfo(data)) return;   // esc
                }
                // Edition mode
                else if (blocked)
                {
                    if (kCode.KeyChar == 71 || kCode.KeyChar == 103) // gG
                    {
                        if (selected != -1) disableEditionMode();
                        else if (SaveContactInfo(data))
                        {
                            SortContacts();
                            return;
                        }
                    }
                    blocked = false;
                    Utilities.ClearAlert();
                }
                else if ((int)kCode.Key == 38) focusField(-1);   // upArrow
                else if ((int)kCode.Key == 40 || kCode.KeyChar == 13) focusField(1); // downArrow
                else if ((int)kCode.Key == 8) Utilities.EraseInField(ref data[i]);   // back
                else if (kCode.KeyChar == 33)   // close key (!)
                {
                    Utilities.ShowAlert("Esperando [g] para terminar edición");
                    blocked = true;
                    continue;
                }
                else if (kCode.KeyChar == 27 && selected == -1) return;  // esc (cancel creation)
                else Utilities.WriteInField(kCode.KeyChar, ref data[i]);
            }
        }

        static bool SaveContactInfo(string[] data)
        {
            bool success = false;
            if (data[0].Trim() == "") Utilities.ShowAlert("Por favor ingrese un nombre de contacto", true);
            else if (data[1].Trim() == "") Utilities.ShowAlert("Se requiere un número de teléfono", true);
            else if (CurrentContact == -1 && contacts.Exists(c => c.Phone == data[1]))
                Utilities.ShowAlert("Ya existe otro contacto con este número de teléfono", true, 5f);
            else
            {
                int i = CurrentContact;
                if (selected == -1)
                {
                    contacts.Add(new Contact());
                    i = contacts.Count - 1;
                }
                contacts[i].Name = data[0];
                contacts[i].Phone = data[1];
                contacts[i].Email = data[2];
                contacts[i].Address = data[3];
                success = true;
            }
            return success;
        }

        static void SearchContact()
        {
            if (contacts.Count == 0)
            {
                Utilities.ShowAlert("No hay contactos para buscar", true);
                return;
            }

            string searchText = "";
            string label = "(Buscar por nombre o teléfono): ";
            string[,] shortcuts = { { "↲", "Buscar" }, { "esc", "Cancelar" } };
            List<Contact> copy = new List<Contact>(contacts);
            int currentScroll = scroll, selection = selected;
            ConsoleKeyInfo k;

            while (true)
            {
                Utilities.ShortcutsMenu(shortcuts);
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.SetCursorPosition(2, Console.WindowHeight - 3);
                Console.Write(label + new string(' ', Console.WindowWidth - label.Length - 4));
                Console.CursorLeft = 2 + label.Length;
                Console.Write(searchText + "█");

                while (true)
                {
                    k = Console.ReadKey(true);
                    if (k.KeyChar == 27) return;
                    else if (k.KeyChar == 13)
                    {
                        Search(searchText);
                        break;
                    }
                    else if (k.Key == ConsoleKey.Backspace) Utilities.EraseInField(ref searchText);
                    else Utilities.WriteInField(k.KeyChar, ref searchText);
                }
                contacts = copy;
                scroll = currentScroll;
                selected = selection;

                if (deletedContacts != null)
                {
                    deletedContacts.ForEach(c => contacts.Remove(c));

                    if (CurrentContact > contacts.Count - 1)
                    {
                        scroll = (int)Math.Ceiling(contacts.Count / (float)MaxContactCount) - 1;
                        selected = contacts.Count - MaxContactCount * scroll - 1;
                    }
                }
                
                deletedContacts = null;
                LoadContactList();
            }
        }

        static void Search(string searchText)
        {
            contacts = contacts.FindAll(c => c.Name.ToLower().Contains(searchText.ToLower()) || c.Phone.Contains(searchText));
            string[,] shortcuts =
            {
                { "↑", "Subir" },
                { "↓", "Bajar" },
                { "↲", "Info" },
                { "e", "Eliminar" },
                { "esc", "Regresar" }
            };
            deletedContacts = new List<Contact>();

            selected = -1;
            scroll = 0;
            Utilities.ClearAlert();

            ConsoleKey k;
            while (true)
            {
                LoadContactList(true);
                Utilities.ShortcutsMenu(shortcuts);

                while (true)
                {
                    k = Console.ReadKey(true).Key;
                    if (k == ConsoleKey.UpArrow) Previus();
                    else if (k == ConsoleKey.DownArrow) Next();
                    else if (k == ConsoleKey.Enter && ContactInfo())
                    {
                        Utilities.WriteTitle("AGENDA DE CONTACTOS");
                        break;
                    }
                    else if (k == ConsoleKey.E)
                    {
                        DeleteContact();
                        Utilities.ShortcutsMenu(shortcuts);
                    }
                    else if (k == ConsoleKey.Escape) return;
                }
            }
        }

        static bool DeleteContact()
        {
            if (contacts.Count == 0) return false;
            if (selected == -1)
            {
                Utilities.ShowAlert("Seleccione el contacto a eliminar", false);
                return false;
            }
            if (!Utilities.ShowDialog("Desea elminar este contacto?", true)) return false;

            if (deletedContacts != null) deletedContacts.Add(contacts[CurrentContact]);
            contacts.RemoveAt(CurrentContact);

            if (CurrentContact > contacts.Count - 1) selected--;
            if (selected == -1 && scroll > 0)
            {
                scroll--;
                selected = MaxContactCount - 1;
            }

            LoadContactList(deletedContacts != null);
            return true;
        }

        static bool ClearContacts()
        {
            if (contacts.Count == 0) return false;
            if (!Utilities.ShowDialog("Se borrarán todos los contactos, desea continuar?", true)) return false;

            contacts.Clear();
            selected = -1;
            scroll = 0;
            LoadContactList();
            return true;
        }

        static bool Close()
        {
            Console.SetCursorPosition(2, Console.WindowHeight - 3);
            bool close = Utilities.ShowDialog("Desea salir del programa?");

            if (close)
            {
                Console.Clear();
                Utilities.Print("Saliendo del programa...");
                Thread.Sleep(1000);
                Console.Clear();
                Console.CursorVisible = true;
            }
            return close;
        }

        static void SortContacts()
        {
            Utilities.Sort(ref contacts, (c1, c2) => c1.Name[0] - c2.Name[0]);
        }
    }
}
