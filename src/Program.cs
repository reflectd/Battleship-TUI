namespace Battleship
{
    class Boat
    {
        public string BoatType { get; private set; }
        public List<(int Left, int Top)> BoatCoordinatesLeft { get; set; } = new List<(int Left, int Top)>();
        
        byte boatLength;
        int startPositionLeft;
        int StartPositionTop;

        public Boat(string _boatType, byte _boatLength)
        {
            BoatType = _boatType;
            boatLength = _boatLength;
            GenerateBoat();
        }

        void GenerateBoat()
        {
            do
            {
                BoatCoordinatesLeft.Clear();
                switch (Program.Rand.Next(0, 2)) // 0 = horizontal, 1 = vertical, 2 = diagonal
                {
                    case 0:
                        startPositionLeft = Program.LEFT_BUFFER + Program.Rand.Next(0, Program.BoardWidth - boatLength + 1) * 2;
                        StartPositionTop = Program.TOP_BUFFER + Program.Rand.Next(0, Program.BoardHeight);

                        for (byte j = 0; j < boatLength; j++)
                            BoatCoordinatesLeft.Add((startPositionLeft + j * 2, StartPositionTop));
                        break;
                    case 1:
                        startPositionLeft = Program.LEFT_BUFFER + Program.Rand.Next(0, Program.BoardWidth) * 2;
                        StartPositionTop = Program.TOP_BUFFER + Program.Rand.Next(0, Program.BoardHeight - boatLength + 1);

                        for (byte j = 0; j < boatLength; j++)
                            BoatCoordinatesLeft.Add((startPositionLeft, StartPositionTop + j));
                        break;
                    /* Uncomment to add the possibility for diagonal boat spawns (and change the switch statement to Program.Rand.Next(0, 3))
                    case 2:
                        switch (Program.Rand.Next(0, 2)) //0 = positive slope, 1 = negative slope
                        {
                            case 0:
                                startPositionLeft = Program.LEFT_BUFFER + Program.Rand.Next(0, Program.BoardWidth - boatLength + 1) * 2;
                                StartPositionTop = Program.TOP_BUFFER + Program.Rand.Next(boatLength - 1, Program.BoardHeight - 1);

                                for (byte j = 0; j < boatLength; j++)
                                    BoatCoordinatesLeft.Add((startPositionLeft + j * 2, StartPositionTop - j));
                                break;
                            case 1:
                                startPositionLeft = Program.LEFT_BUFFER + Program.Rand.Next(0, Program.BoardWidth - boatLength + 1) * 2;
                                StartPositionTop = Program.TOP_BUFFER + Program.Rand.Next(0, Program.BoardHeight - boatLength + 1);

                                for (byte j = 0; j < boatLength; j++)
                                    BoatCoordinatesLeft.Add((startPositionLeft + j * 2, StartPositionTop + j));
                                break;
                        }
                        break;
                    */
                }
            } while (Program.RemainingBoatLocations
                .Any(typeOfBoat => typeOfBoat.Value
                    .Any(existingBoat => existingBoat.BoatCoordinatesLeft
                        .Any(coords => this.BoatCoordinatesLeft
                            .Contains(coords))))); // Re-generate the boat if it overlaps with an existing boat
            
        }
    }
    internal class Program
    {
        public static Dictionary<string, List<Boat>> RemainingBoatLocations = new Dictionary<string, List<Boat>>();
        public static readonly Random Rand = new Random();
        public const byte LEFT_BUFFER = 7; // Terminal column number where board grid starts
        public const byte TOP_BUFFER = 8; // Terminal row number where board grid starts
        public static byte BoardWidth = 0;
        public static byte BoardHeight = 0;
		
        const byte MIN_BOARDWIDTH = 10;
        const byte MAX_BOARDWIDTH = 26;
        const byte MIN_BOARDHEIGHT = 10;
        const byte MAX_BOARDHEIGHT = 35;
        const byte CARRIER_LENGTH = 5;
        const byte BATTLESHIP_LENGTH = 4;
        const byte SUBMARINE_LENGTH = 3;
        const byte PATROL_BOAT_LENGTH = 2;
        const string CARRIER_STR = "Carrier";
        const string BATTLESHIP_STR = "Battleship";
        const string SUBMARINE_STR = "Submarine";
        const string PATROL_BOAT_STR = "Patrol Boat";
        
        static (string TypeName, byte TypeLength)[] boatTypes = new (string, byte)[] // I use an array of tuples rather than a dictionary because I want the order to be consistent (I loop through them to show the remaining boat stats)
        {
            (CARRIER_STR, CARRIER_LENGTH),
            (BATTLESHIP_STR, BATTLESHIP_LENGTH),
            (SUBMARINE_STR, SUBMARINE_LENGTH),
            (PATROL_BOAT_STR, PATROL_BOAT_LENGTH)
        };
        
        static char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        static bool exitBool = false;
        static bool gameOverBool = false;
        static ConsoleKey keyStroke;
        static ConsoleKey questionKey;
        static ConsoleKey[] acceptableQuestionInputs = { ConsoleKey.Y, ConsoleKey.Enter, ConsoleKey.N };
        static bool inputValidated;
        static byte maxPossibleBoardWidth; // Defined based on terminal's window width
        static byte maxPossibleBoardHeight; // Defined based on terminal's window height
        static ushort shotCounter = 0;
        static (int Left, int Top) cursorCoords;
        static List<(int Left, int Top)> coordinatesShot = new List<(int Left, int Top)>();
        static bool hitBoat;

        static void WriteTitle()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("      ___   _ _____ _____ _    ___ ___ _  _ ___ ___ ");
            Console.WriteLine("     | _ ) /_\\_   _|_   _| |  | __/ __| || |_ _| _ \\");
            Console.WriteLine("     | _ \\/ _ \\| |   | | | |__| _|\\__ \\ __ || ||  _/");
            Console.WriteLine("     |___/_/ \\_\\_|   |_| |____|___|___/_||_|___|_|  ");
            Console.ResetColor();
        }

        static void UpdateStats()
        {
            for (byte i = 0; i < boatTypes.Length; i++)
            {
                Console.ForegroundColor = RemainingBoatLocations[boatTypes[i].TypeName].Count == 0 ? ConsoleColor.Red : Console.ForegroundColor;
                Console.SetCursorPosition(LEFT_BUFFER + BoardWidth * 2 + 8, TOP_BUFFER + i + 6);
                Console.Write("{0, -14} {1, -1}", boatTypes[i].TypeName + "s", RemainingBoatLocations[boatTypes[i].TypeName].Count);
                Console.ResetColor();
            }

            Console.SetCursorPosition(BoardWidth * 2 + 15, TOP_BUFFER + boatTypes.Length + 7);
            Console.Write("Shot Counter: " + shotCounter);
            Console.SetCursorPosition(cursorCoords.Left, cursorCoords.Top);
        }

        static void QuitQuestion(string question)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.SetCursorPosition(0, TOP_BUFFER + BoardHeight + 3);
            Console.Write(question);
            do
            {
                questionKey = Console.ReadKey(true).Key;
                if (questionKey == ConsoleKey.Y || questionKey == ConsoleKey.Enter)
                {
                    exitBool = true;
                }
                else if (questionKey == ConsoleKey.N)
                {
                    Console.CursorLeft = 0;
                    Console.Write("                                                ");
                    exitBool = false;
                }
            } while (!acceptableQuestionInputs.Contains(questionKey));
            Console.ResetColor();
            Console.SetCursorPosition(cursorCoords.Left, cursorCoords.Top);
        }
		
        static void Main(string[] args)
        {
            do
            {
                gameOverBool = false;
                coordinatesShot.Clear();
                RemainingBoatLocations.Clear();
                Console.ResetColor();
                shotCounter = 0;
                cursorCoords = (LEFT_BUFFER, TOP_BUFFER); // Set cursor coords to top left corner of the game board

                do {
                    Console.Clear();
                    maxPossibleBoardWidth = (byte)Math.Min(MAX_BOARDWIDTH, (Console.WindowWidth - 51) / 2);
                    maxPossibleBoardHeight = (byte)Math.Min(MAX_BOARDHEIGHT, Console.WindowHeight - 13);
                    if (maxPossibleBoardWidth < MIN_BOARDWIDTH || maxPossibleBoardHeight < MIN_BOARDHEIGHT)
                    {
                        Console.WriteLine($"Your terminal's window size is too small, please resize it to be at least {MIN_BOARDWIDTH * 2 + 51} columns wide and {23} rows tall.");
                        Console.WriteLine($"Current terminal size: {Console.WindowWidth} columns x {Console.WindowHeight} rows");
                        Console.Write("Press a key once you've resized your terminal to try again... ");
                        Console.ReadKey(true);
                    }
                } while (maxPossibleBoardWidth < MIN_BOARDWIDTH || maxPossibleBoardHeight < MIN_BOARDHEIGHT);
                
                WriteTitle();

                do
                {
                    Console.Write($"\nEnter board width (min. {MIN_BOARDWIDTH}, max. {maxPossibleBoardWidth}) : ");
                    inputValidated = byte.TryParse(Console.ReadLine(), out BoardWidth) && BoardWidth >= MIN_BOARDWIDTH && BoardWidth <= maxPossibleBoardWidth;
                    if (!inputValidated)
                        Console.WriteLine($"\nThe value you have entered is either not a number, not between {MIN_BOARDWIDTH} and {maxPossibleBoardWidth}, or is not an integer.");
                } while (!inputValidated);

                do
                {
                    Console.Write($"\nEnter board height (min. {MIN_BOARDHEIGHT}, max. {maxPossibleBoardHeight}) : ");
                    inputValidated = byte.TryParse(Console.ReadLine(), out BoardHeight) && BoardHeight >= MIN_BOARDHEIGHT && BoardHeight <= maxPossibleBoardHeight;
                    if (!inputValidated) Console.WriteLine($"\nThe value you have entered is either not a number, not between {MIN_BOARDHEIGHT} and {maxPossibleBoardHeight}, or is not an integer.");
                } while (!inputValidated);

                // Creating the boats
                foreach ((string TypeName, byte TypeLength) boatType in boatTypes)
                {
                    byte boatQuantity = Convert.ToByte(Rand.Next(1, 3));
                    RemainingBoatLocations[boatType.TypeName] = new List<Boat>();
                    for (byte i = 0; i < boatQuantity; i++)
                        RemainingBoatLocations[boatType.TypeName].Add(new Boat(boatType.TypeName, boatType.TypeLength));
                }

                WriteTitle();

                // Draw the board
                for (int i = 1; i <= BoardHeight + 3; i++)
                {
                    if (i == 1) // Write letters on first iteration
                    {
                        Console.SetCursorPosition(LEFT_BUFFER, TOP_BUFFER - 2);
                        for (int j = 1; j <= BoardWidth; j++) Console.Write(alphabet[j - 1] + " ");
						Console.WriteLine();
                    }
                    else if (i == 2) // Draw top border on second iteration
                    {
                        Console.CursorLeft = 5;
                        Console.WriteLine("╔═" + new String('═', BoardWidth * 2) + "╗");
                    }
                    else if (i == BoardHeight + 3) // Draw bottom border on last iteration
                    {
                        Console.CursorLeft = 5;
                        Console.Write("╚═" + new String('═', BoardWidth * 2) + "╝");
                    }
                    else
                    {
                        Console.CursorLeft = 2;
                        Console.WriteLine((i - 2) + ((i - 2) < 10 ? "  " : " ") + "║ " + String.Concat(Enumerable.Repeat(". ", BoardWidth)) + "║");
                    }
                }

                // Write instructions
                Console.SetCursorPosition(LEFT_BUFFER + BoardWidth * 2 + 8, TOP_BUFFER - 2);
                Console.Write("Instructions");
                Console.SetCursorPosition(LEFT_BUFFER + BoardWidth * 2 + 8, TOP_BUFFER - 1);
                Console.Write("---------------");
                Console.SetCursorPosition(LEFT_BUFFER + BoardWidth * 2 + 8, TOP_BUFFER);
                Console.Write("{0, -15}{1, -24}", "Movement", "Arrow keys or WASD");
                Console.SetCursorPosition(LEFT_BUFFER + BoardWidth * 2 + 8, TOP_BUFFER + 1);
                Console.Write("{0, -15}{1, -24}", "Shoot", "Spacebar or Enter");
                Console.SetCursorPosition(LEFT_BUFFER + BoardWidth * 2 + 8, TOP_BUFFER + 2);
                Console.Write("{0, -15}{1, -24}", "Quit", "Escape key");

                // Write remaining boats
                Console.SetCursorPosition(LEFT_BUFFER + BoardWidth * 2 + 8, TOP_BUFFER + 4);
                Console.Write("Remaining Boats");
                Console.SetCursorPosition(LEFT_BUFFER + BoardWidth * 2 + 8, TOP_BUFFER + 5);
                Console.Write("---------------");
                UpdateStats();

                // Read game key inputs loop
                do
                {
                    cursorCoords = Console.GetCursorPosition();
                    keyStroke = Console.ReadKey(true).Key;
                    switch (keyStroke)
                    {
                        case ConsoleKey.Escape:
                            QuitQuestion("Do you really want to quit the game ? (Y/n) : ");
                            break;

                        case (ConsoleKey.Spacebar or ConsoleKey.Enter):
                            if (coordinatesShot.Contains(cursorCoords)) break;

							shotCounter++;							
							coordinatesShot.Add(cursorCoords);

							hitBoat = false;
							foreach (KeyValuePair<string, List<Boat>> RemainingBoatLocationsOfType in RemainingBoatLocations.ToList())
							{
								foreach (Boat boat in RemainingBoatLocationsOfType.Value.ToList()) {
                                    hitBoat = boat.BoatCoordinatesLeft.Contains(cursorCoords) ? true : false;
                                    if (!hitBoat) continue;

                                    boat.BoatCoordinatesLeft.Remove(cursorCoords);
                                    if (boat.BoatCoordinatesLeft.Count == 0)
                                        RemainingBoatLocationsOfType.Value.Remove(boat);
                                    break;
                                }
                                if (hitBoat) break;
							}
                            
                            if (hitBoat)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write("x");
                            }
                            else
							{
								Console.ForegroundColor = ConsoleColor.Blue;
								Console.Write("■");
							}

							Console.ResetColor();
							Console.CursorLeft = cursorCoords.Left;
                            UpdateStats();
                            gameOverBool = RemainingBoatLocations.All(boatType => boatType.Value.Count == 0) ? true : false;
                            break;

                        case (ConsoleKey.LeftArrow or ConsoleKey.A):
                            Console.CursorLeft = cursorCoords.Left > LEFT_BUFFER ? cursorCoords.Left - 2 : Console.CursorLeft;
                            break;

                        case (ConsoleKey.RightArrow or ConsoleKey.D):
                            Console.CursorLeft = cursorCoords.Left < LEFT_BUFFER + BoardWidth * 2 - 2 ? cursorCoords.Left + 2 : Console.CursorLeft;
                            break;

                        case (ConsoleKey.UpArrow or ConsoleKey.W):
                            Console.CursorTop = cursorCoords.Top > TOP_BUFFER ? cursorCoords.Top - 1 : Console.CursorTop;
                            break;

                        case (ConsoleKey.DownArrow or ConsoleKey.S):
                            Console.CursorTop = cursorCoords.Top < TOP_BUFFER + BoardHeight - 1 ? cursorCoords.Top + 1 : Console.CursorTop;
                            break;
                    }
                } while (!exitBool && !gameOverBool);

                if (gameOverBool)
                    QuitQuestion("Game over! All boats have been sunk. Quit? (Y/n) : ");
            } while (!exitBool);
            Console.ResetColor();
            Console.Clear();
        }
    }
}