using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //int n = 32;
            //int d = 2;


            //int a = Convert.ToInt32(Math.Log2(n));
            //int w = Convert.ToInt32(Math.Log2(d));
            //int x = 0;

            //for (int i = (a - 1); i >= w; i--)
            //{
            //    x = x + Convert.ToInt32(Math.Pow(2, i));
            //}

            //int y = 2 * x;

            ////Console.WriteLine(x);
            ////Console.WriteLine(y);

            //for (int i = 0; i <= 60; i++)
            //{
            //    int resultWin = win(i, n, d, x, y, w, a);
            //    int resultLose = lose(i, n, d, x, y, w, a);
            //    Console.WriteLine(i + "\tw:" + resultWin + "\tl:" + resultLose);
            //}

            //Console.WriteLine(roundName(1, 32, 8));
            //Console.WriteLine(roundName(17, 32, 8));
            //Console.WriteLine(roundName(32, 32, 8));
            //Console.WriteLine(roundName(40, 32, 8));
            //Console.WriteLine(roundName(59, 32, 8));

            Console.WriteLine(CheckRound(8, 16));
            Console.WriteLine(CheckRound(29, 32));
            Console.WriteLine(CheckRound(12, 16));
            Console.WriteLine(CheckRound(15, 16));
        }

        public static string roundName(int matchNum, int playerNumber, int finalSinglePlayer)
        {
            int roundNumber = Convert.ToInt32(Math.Log2(playerNumber / 2));
            int finalSingleRound = roundNumber - Convert.ToInt32(Math.Log2(finalSinglePlayer)) + 1;
            int numberMatchEachRound;
            int count = 0;
            int winRound = 0;
            int loseRound = 0;

            for (int i = 0; i <= finalSingleRound; i++)
            {
                winRound++;
                numberMatchEachRound = Convert.ToInt32(Math.Pow(2, roundNumber - i));
                for (int j = 0; j < numberMatchEachRound; j++)
                {
                    if (i == 0)
                    {
                        if (++count == matchNum)
                        {
                            return "W" + winRound;
                        }
                    }
                    else
                    {
                        if (++count == matchNum)
                        {
                            return "W" + winRound;
                        }
                    }
                }
            }
            for (int i = 1; i <= finalSingleRound; i++)
            {
                loseRound++;
                numberMatchEachRound = Convert.ToInt32(Math.Pow(2, roundNumber - i));
                for (int j = 0; j < numberMatchEachRound; j++)
                {
                    if (++count == matchNum)
                    {
                        return "L" + loseRound;
                    }
                }
                loseRound++;
                for (int j = 0; j < numberMatchEachRound; j++)
                {
                    if (++count == matchNum)
                    {
                        return "L" + loseRound;
                    }
                }
            }

            for (int i = finalSingleRound; i <= roundNumber; i++)
            {
                winRound++;
                numberMatchEachRound = Convert.ToInt32(Math.Pow(2, roundNumber - i));
                if (i == finalSingleRound)
                {
                    for (int j = 0; j < numberMatchEachRound; j++)
                    {
                        if (++count == matchNum)
                        {
                            return "W" + winRound;
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < numberMatchEachRound; j++)
                    {
                        if (++count == matchNum)
                        {
                            return "W" + winRound;
                        }
                    }
                }

            }
            return "";
        }

        public static int CheckRound(int currentMatch, int bracketSize)
        {
            int logForCal = (int)Math.Log2(bracketSize);
            int temp = 0;
            for (int i = 1; i <= logForCal; i++)
            {
                temp += bracketSize / (int)Math.Pow(2, i);
                if (temp >= currentMatch)
                {
                    return i;
                }
            }
            return 0;
        }

        public static int NextMatchSingle(int currentMatch, int bracketSize)
        {
            int nextMatch;
            if (currentMatch % 2 == 0)
            {
                nextMatch = currentMatch / 2 + bracketSize / 2;
            }
            else
            {
                nextMatch = currentMatch / 2 + 1 + bracketSize / 2;
            }
            return nextMatch;
        }

        public static int win(int m, int n, int d, int x, int y, int w, int a)
        {
            if (m % 2 == 1 && m <= x)
            {
                return (n / 2 + (m + 1) / 2);
            }
            else if (m % 2 == 0 && m <= x)
            {
                return n / 2 + m / 2;
            }
            else if (m > x && m <= (x + Convert.ToInt32(Math.Pow(2, (w - 1)))))
            {
                return m * 2 + d / 2 - (m - x);
            }
            else if (m % 2 == 1 && m > (y + Convert.ToInt32(Math.Pow(2, (w - 1)))))
            {
                return Convert.ToInt32(0.5 * y + 0.75 * d + (m + 1) / 2);
            }
            else if (m % 2 == 0 && m > (y + Convert.ToInt32(Math.Pow(2, (w - 1)))))
            {
                return Convert.ToInt32(0.5 * y + 0.75 * d + m / 2);
            }
            else if (m > y && m <= (y + d / 2))
            {
                if (m > (y + d / 4))
                {
                    if (w == 1)
                    {
                        return m + d / 4 + 1;
                    }
                    return m + d / 4;
                }
                else
                {
                    return m + d - d / 4;
                }
            }
            else
            {
                int from = x + d / 2 + 1;
                int to = x + d / 2 + Convert.ToInt32(Math.Pow(2, a - 2));
                for (int i = (a - 2); i >= (w - 1); i--)
                {
                    if (m >= from && m <= to)
                    {
                        return m + Convert.ToInt32(Math.Pow(2, i));
                    }
                    else if (m > to && m <= (to + Convert.ToInt32(Math.Pow(2, i))))
                    {
                        if ((m % 2 == 0 && d == 2) || m % 2 == 1 && d != 2)
                        {
                            return Convert.ToInt32((m + 1 - to) / 2 + to + Math.Pow(2, i));
                        }
                        else
                        {
                            return Convert.ToInt32((m - to) / 2 + to + Math.Pow(2, i));
                        }
                    }
                    from += Convert.ToInt32(2 * Math.Pow(2, i));
                    to += Convert.ToInt32(1.5 * Math.Pow(2, i));
                }
            }
            return 0;
        }

        public static int lose(int m, int n, int d, int x, int y, int w, int a)
        {
            if (m % 2 == 1 && m <= n / 2)
            {
                return ((x + d / 2) + (m + 1) / 2);
            }
            else if (m % 2 == 0 && m <= n / 2)
            {
                return (x + d / 2) + m / 2;
            }
            else if (m > n / 2 && m <= 0.75 * n)
            {
                return (x + d / 2 + n + (1 - m));
            }
            else if (m > 0.75 * n && m <= x)
            {
                for (int i = a - 3; i >= w; i--)
                {
                    int count1 = 0;
                    for (int j = a - 1; j >= i + 1; j--)
                    {
                        count1 += Convert.ToInt32(Math.Pow(2, j));
                    }

                    int count2 = count1 + Convert.ToInt32(Math.Pow(2, i));

                    if (m > count1 && m <= count2)
                    {
                        if (m % 2 == 1)
                        {
                            return x + d / 2 + count1 - (count2 - m) + 1;
                        }
                        else if (m % 2 == 0)
                        {
                            return x + d / 2 + count1 - (count2 - m) - 1;
                        }
                    }
                }

            }
            else if (m % 2 == 1 && m > x && m <= (x + Convert.ToInt32(Math.Pow(2, (w - 1)))))
            {
                if (w == 1)
                {
                    return y + m - x;
                }
                return y + (m - x + 1);
            }
            else if (m % 2 == 0 && m > x && m <= (x + Convert.ToInt32(Math.Pow(2, (w - 1)))))
            {
                return y + (m - x - 1);
            }
            return 0;
        }
    }
}