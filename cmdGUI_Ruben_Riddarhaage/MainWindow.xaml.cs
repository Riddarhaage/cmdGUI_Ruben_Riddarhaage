using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace cmdGUI_Ruben_Riddarhaage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Process cmdProcess;
        private StreamWriter cmdStreamWriter;
        private bool isDeleteMode = false;
        public MainWindow()
        {
            InitializeComponent();
            InitializeCmdProcess();
            SwitchLanguage("en");
        }

        private void InitializeCmdProcess()
        {
            cmdProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            cmdProcess.OutputDataReceived += CmdProcess_OutputDataReceived;
            //cmdProcess.ErrorDataReceived += CmdProcess_ErrorDataReceived;

            cmdProcess.Start();
            cmdProcess.BeginOutputReadLine();
            cmdProcess.BeginErrorReadLine();

            cmdStreamWriter = cmdProcess.StandardInput;
            currentDirectoryTextBlock.Text = Directory.GetCurrentDirectory();
        }
        private void CmdProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    string match = Regex.Match(e.Data, @"[A-Z]:\\[^\r\n]+").Value;
                    if(match != "")
                    {
                        string currentPath = match.Split('>')[0];
                        currentDirectoryTextBlock.Text = currentPath;
                        currentDirectoryTextBlock.ScrollToEnd();

                    }

                }

            });
            // This event is called when cmd.exe produces output.
            if (!string.IsNullOrEmpty(e.Data))
            {
                AppendTextToOutput(e.Data);
            }
        }
        private void AppendTextToOutput(string text)
        {
            // Append the text to the output TextBox and set the currentPath using regex
            Application.Current.Dispatcher.Invoke(() =>
            {
                output.AppendText(text + Environment.NewLine);
                output.ScrollToEnd();

            });
        }

        private void CmdProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            
            // This event is called when cmd.exe produces an error.
            if (!string.IsNullOrEmpty(e.Data))
            {
                AppendTextToOutput(e.Data);
            }
        }

        private void CommandButton_Click(object sender, RoutedEventArgs e)
        {
            string command = commandsInput.Text;

            if (!string.IsNullOrWhiteSpace(command))
            {
                cmdStreamWriter.WriteLine(command);
                commandsInput.Text = "";
            }
        }

        private void ToggleDeleteMode_Click(object sender, RoutedEventArgs e)
        {
            isDeleteMode = !isDeleteMode;

            foreach (UIElement element in dynamicButtonPanel.Children)
            {
                if (element is Button dynamicButton)
                {
                    if (isDeleteMode)
                    {
                        dynamicButton.Style = FindResource("DeleteModeButtonStyle") as Style;
                    }
                    else
                    {
                        dynamicButton.Style = FindResource("CommonButtonStyle") as Style;
                        dynamicButton.Click -= DeleteDynamicButton_Click;
                    }
                }
            }
        }
        private void DeleteDynamicButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle the delete button click here (e.g., remove the button)
            dynamicButtonPanel.Children.Remove(sender as UIElement);
        }

        private void CommandsInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CommandButton_Click(sender, e);
            }
        }
        private void SwitchLanguage(string languageCode)
        {
            ResourceDictionary dictionary = new()
            {
                Source = languageCode switch
                {
                    "en" => new Uri("..\\LanguageResource.en.xaml", UriKind.Relative),
                    "sv" => new Uri("..\\LanguageResource.sv.xaml", UriKind.Relative),
                    _ => new Uri("..\\LanguageResource.en.xaml", UriKind.Relative),
                }
            };
            this.Resources.MergedDictionaries.Add(dictionary);
        }

        private void MakeNewCommandInputVisible_Click(object sender, RoutedEventArgs e)
        {
            object buttonLimitResource = this.Resources["buttonLimitMessage"];

            if (dynamicButtonPanel.Children.Count >= 10)
            {
                MessageBox.Show($"{buttonLimitResource}");
            }
            else
            {
                newCommandWindow.Visibility = Visibility.Visible;
            }
        }
        private void MakeNewCommandInputHidden_Click(object sender, RoutedEventArgs e)
        {
            newCommandWindow.Visibility = Visibility.Hidden;
            newCommandInput.Text = "";

        }
        private void CreateDynamicButton_Click(object sender, RoutedEventArgs e)
        {
            string buttonContent = newCommandInput.Text;

            if (!string.IsNullOrWhiteSpace(buttonContent))
            {
                Button newButton = new()
                {
                    Content = buttonContent,
                    Margin = new Thickness(0,10,0,0),
                    FontSize = 16,
                    Style = (Style)FindResource("CommonButtonStyle")
                };

                newButton.Click += DynamicButton_Click;

                dynamicButtonPanel.Children.Add(newButton);

                newCommandWindow.Visibility = Visibility.Hidden;
                newCommandInput.Text = "";
            }
        }
        private void DynamicButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isDeleteMode)
            {
                commandsInput.Text = ((Button)sender).Content.ToString() + " ";
                commandsInput.Focus();
                commandsInput.CaretIndex = commandsInput.Text.Length;
            }
        }
        private void Button_Click_Language_EN(object sender, RoutedEventArgs e)
        {
            SwitchLanguage("en");
        }
        private void Button_Click_Language_SV(object sender, RoutedEventArgs e)
        {
            SwitchLanguage("sv");
        }
        private void Cd_Click(object sender, RoutedEventArgs e)
        {
            commandsInput.Text = "cd ";
            commandsInput.Focus();
            commandsInput.CaretIndex = commandsInput.Text.Length;
        }
        private void Mkdir_Click(object sender, RoutedEventArgs e)
        {
            commandsInput.Text = "mkdir ";
            commandsInput.Focus();
            commandsInput.CaretIndex = commandsInput.Text.Length;
        }
        private void CdDoubleDot_Click(object sender, RoutedEventArgs e)
        {
            commandsInput.Text = "cd..";
            commandsInput.Focus();
            commandsInput.CaretIndex = commandsInput.Text.Length;
        }
        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            commandsInput.Text = "copy ";
            commandsInput.Focus();
            commandsInput.CaretIndex = commandsInput.Text.Length;
        }
        private void Open_Help_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow helpWindow = new();
            helpWindow.Show();
            helpWindow.Resources.MergedDictionaries.Add(this.Resources);
        }
    }


}
