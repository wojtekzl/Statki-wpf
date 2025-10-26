using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Statki
{
    public partial class MainWindow : Window
    {
        private int[,] planszaGracza = new int[10, 10];
        private int[,] planszaAI = new int[10, 10];
        private Button[,] przyciskiAI = new Button[10, 10];
        private Button[,] przyciskiGracza = new Button[10, 10];
        private Random generatorLosowy = new Random();

        private readonly string[] naglowkiKolumn = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
        private readonly string[] naglowkiWierszy = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };

        private Dictionary<int, int> statkiDoRozmieszczenia = new Dictionary<int, int>()
        {
            {4, 1},
            {3, 2},
            {2, 3},
            {1, 4}
        };

        private bool czyRozmieszczanieStatkow = true;
        private int czesciStatkowGracza = 0;
        private int czesciStatkowAI = 0;

        public MainWindow()
        {
            InitializeComponent();
            InicjalizujPlanszeZNaglowkami(PlanszaGraczaZNaglowkami, przyciskiGracza, ObsługaKliknieciaPolaGracza);
            InicjalizujPlanszeZNaglowkami(PlanszaAIZNaglowkami, przyciskiAI, ObsługaKliknieciaPolaAI);
            RozmiescStatkiAI();
        }

        private void InicjalizujPlanszeZNaglowkami(Grid plansza, Button[,] przyciski, RoutedEventHandler obslugaKlikniecia)
        {
            plansza.Children.Clear();
            plansza.RowDefinitions.Clear();
            plansza.ColumnDefinitions.Clear();

            for (int i = 0; i <= 10; i++)
            {
                plansza.RowDefinitions.Add(new RowDefinition());
                plansza.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int kol = 1; kol <= 10; kol++)
            {
                TextBlock naglowek = new TextBlock
                {
                    Text = naglowkiKolumn[kol - 1],
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                Grid.SetRow(naglowek, 0);
                Grid.SetColumn(naglowek, kol);
                plansza.Children.Add(naglowek);
            }

            for (int wiersz = 1; wiersz <= 10; wiersz++)
            {
                TextBlock naglowek = new TextBlock
                {
                    Text = naglowkiWierszy[wiersz - 1],
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(3, 0, 5, 0)
                };
                Grid.SetRow(naglowek, wiersz);
                Grid.SetColumn(naglowek, 0);
                plansza.Children.Add(naglowek);
            }

            for (int wiersz = 0; wiersz < 10; wiersz++)
            {
                for (int kol = 0; kol < 10; kol++)
                {
                    Button przycisk = new Button
                    {
                        Tag = new Point(wiersz, kol),
                        MinWidth = 40,
                        MinHeight = 40,
                        Margin = new Thickness(1),
                        Background = Brushes.LightGray
                    };
                    przycisk.Click += obslugaKlikniecia;
                    Grid.SetRow(przycisk, wiersz + 1);
                    Grid.SetColumn(przycisk, kol + 1);
                    plansza.Children.Add(przycisk);
                    przyciski[wiersz, kol] = przycisk;
                }
            }
        }

        private void ObsługaKliknieciaPolaGracza(object sender, RoutedEventArgs e)
        {
            if (!czyRozmieszczanieStatkow) return;

            Button kliknietyPrzycisk = sender as Button;
            Point pozycja = (Point)kliknietyPrzycisk.Tag;
            int wiersz = (int)pozycja.X;
            int kolumna = (int)pozycja.Y;

            if (SelektorDlugosciStatku.SelectedItem is ComboBoxItem wybranyElement && int.TryParse(wybranyElement.Content?.ToString(), out int dlugoscStatku))
            {
                bool czyPionowo = SelektorOrientacjiStatku?.SelectedIndex == 1;

                if (statkiDoRozmieszczenia[dlugoscStatku] <= 0)
                {
                    MessageBox.Show("Wszystkie statki o długości " + dlugoscStatku.ToString() + " zostały już ustawione.", "Limit osiągnięty", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!CzyMoznaUmiescicStatek(planszaGracza, wiersz, kolumna, dlugoscStatku, czyPionowo))
                {
                    MessageBox.Show("Nie można ustawić statku obok innego lub poza planszą.", "Błąd ustawiania", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                for (int i = 0; i < dlugoscStatku; i++)
                {
                    int w = wiersz + (czyPionowo ? i : 0);
                    int k = kolumna + (czyPionowo ? 0 : i);
                    planszaGracza[w, k] = 1;
                    przyciskiGracza[w, k].Background = Brushes.Gray;
                }

                statkiDoRozmieszczenia[dlugoscStatku]--;
                czesciStatkowGracza += dlugoscStatku;
            }
            else
            {
                MessageBox.Show("Najpierw wybierz długość statku.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private bool CzyMoznaUmiescicStatek(int[,] plansza, int wiersz, int kolumna, int dlugosc, bool pionowo)
        {
            if (pionowo && wiersz + dlugosc > 10) return false;
            if (!pionowo && kolumna + dlugosc > 10) return false;

            for (int i = -1; i <= dlugosc; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int w = wiersz + (pionowo ? i : j);
                    int k = kolumna + (pionowo ? j : i);
                    if (w >= 0 && w < 10 && k >= 0 && k < 10)
                    {
                        if (plansza[w, k] != 0) return false;
                    }
                }
            }
            return true;
        }

        private void RozmiescStatkiAI()
        {
            int[] rozmiaryStatkow = { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };
            foreach (int dlugosc in rozmiaryStatkow)
            {
                bool umieszczono = false;
                while (!umieszczono)
                {
                    bool pionowo = generatorLosowy.Next(2) == 0;
                    int wiersz = generatorLosowy.Next(10);
                    int kolumna = generatorLosowy.Next(10);

                    if (CzyMoznaUmiescicStatek(planszaAI, wiersz, kolumna, dlugosc, pionowo))
                    {
                        for (int i = 0; i < dlugosc; i++)
                        {
                            int w = wiersz + (pionowo ? i : 0);
                            int k = kolumna + (pionowo ? 0 : i);
                            planszaAI[w, k] = 1;
                        }
                        czesciStatkowAI += dlugosc;
                        umieszczono = true;
                    }
                }
            }
        }

        private void PrzyciskRozpocznijGre_Click(object sender, RoutedEventArgs e)
        {
            foreach (var para in statkiDoRozmieszczenia)
            {
                if (para.Value > 0)
                {
                    MessageBox.Show("Nie ustawiono wszystkich statków!", "Ustaw statki", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            czyRozmieszczanieStatkow = false;
            MessageBox.Show("Gra rozpoczęta!", "Start", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PrzyciskZasadyGry_Click(object sender, RoutedEventArgs e)
        {
            string zasady =
                "Zasady gry w Statki:\n" +
                "- Każdy gracz ma planszę 10x10 i rozmieszcza swoje statki.\n" +
                "- Statki: 1x 4-masztowiec, 2x 3-masztowiec, 3x 2-masztowiec, 4x 1-masztowiec.\n" +
                "- Statki nie mogą się stykać (nawet rogami).\n" +
                "- Statki mogą być poziome lub pionowe.\n" +
                "- Gracze strzelają na zmianę.\n" +
                "- Trafienie = czerwona kratka, pudło = niebieska.\n" +
                "- Trafiony statek zostaje zatopiony, jeśli wszystkie jego pola zostały trafione.\n" +
                "- Gra kończy się, gdy zatopisz wszystkie statki przeciwnika.";

            MessageBox.Show(zasady, "Zasady gry", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ObsługaKliknieciaPolaAI(object sender, RoutedEventArgs e)
        {
            if (czyRozmieszczanieStatkow) return;

            Button kliknietyPrzycisk = (Button)sender;
            Point pozycja = (Point)kliknietyPrzycisk.Tag;
            int wiersz = (int)pozycja.X;
            int kolumna = (int)pozycja.Y;

            if (!kliknietyPrzycisk.IsEnabled) return;

            if (planszaAI[wiersz, kolumna] == 1)
            {
                kliknietyPrzycisk.Background = Brushes.Red;
                kliknietyPrzycisk.IsHitTestVisible = false;
                planszaAI[wiersz, kolumna] = -1;
                czesciStatkowAI--;

                if (czesciStatkowAI == 0)
                {
                    var wynik = MessageBox.Show("Wygrałeś! Czy chcesz zagrać ponownie?", "Koniec gry", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (wynik == MessageBoxResult.Yes)
                        ResetujGre();
                    else
                        Application.Current.Shutdown();
                }
            }
            else if (planszaAI[wiersz, kolumna] == 0)
            {
                kliknietyPrzycisk.Background = Brushes.LightBlue;
                kliknietyPrzycisk.IsHitTestVisible = false;
                planszaAI[wiersz, kolumna] = -2;
                TuraAI();
            }
        }

        private void TuraAI()
        {
            while (true)
            {
                int wiersz = generatorLosowy.Next(10);
                int kolumna = generatorLosowy.Next(10);

                if (planszaGracza[wiersz, kolumna] == 1)
                {
                    przyciskiGracza[wiersz, kolumna].Background = Brushes.Red;
                    planszaGracza[wiersz, kolumna] = -1;
                    czesciStatkowGracza--;

                    if (czesciStatkowGracza == 0)
                    {
                        var wynik = MessageBox.Show("Przegrałeś! Czy chcesz zagrać ponownie?", "Koniec gry", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (wynik == MessageBoxResult.Yes)
                            ResetujGre();
                        else
                            Application.Current.Shutdown();
                    }
                    break;
                }
                else if (planszaGracza[wiersz, kolumna] == 0)
                {
                    przyciskiGracza[wiersz, kolumna].Background = Brushes.LightBlue;
                    planszaGracza[wiersz, kolumna] = -2;
                    break;
                }
            }
        }

        private void ResetujGre()
        {
            planszaGracza = new int[10, 10];
            planszaAI = new int[10, 10];
            statkiDoRozmieszczenia = new Dictionary<int, int>()
            {
                {4, 1},
                {3, 2},
                {2, 3},
                {1, 4}
            };
            czesciStatkowGracza = 0;
            czesciStatkowAI = 0;
            czyRozmieszczanieStatkow = true;

            foreach (var przycisk in przyciskiGracza)
            {
                przycisk.Background = Brushes.LightGray;
                przycisk.IsHitTestVisible = true;
            }

            foreach (var przycisk in przyciskiAI)
            {
                przycisk.Background = Brushes.LightGray;
                przycisk.IsHitTestVisible = true;
            }

            RozmiescStatkiAI();
        }
    }
}
