using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace wordle
{
    public partial class frmMain : Form
    {

        List <Label> letterLabel = new List<Label>();
        int curCol=0, curRow=0;
        int guesses = 1;
        string word = "";
        HashSet<string> wordList = new HashSet<string>();
        
        public frmMain()
        {
            InitializeComponent();
            generateLabels();
            loadWords(); 
            this.KeyPress += frmMain_KeyPress;
            this.KeyPreview = true;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

        }

        public void loadWords()
        {
            string tempWord = "";
            Random rndNum = new Random();
            int element = 0;
            try
            {
                string[] allwords = File.ReadAllLines("wordSelect.txt");
                foreach(string temp in allwords)
                {
                    tempWord = temp.Trim(); //remove whitespaces
                    tempWord = tempWord.ToUpperInvariant(); //for diff keyboards
                    wordList.Add(tempWord);
                }
            }
            catch(IOException ex)
            {
                MessageBox.Show("Problem with the wordle file. wordSelect.txt");

            }
            element = rndNum.Next(0, wordList.Count);
            word = wordList.ElementAt(element);
            //word = "GRASS";

            //MessageBox.Show("The word is: " + word); //check when running
            try
            {
                string[] allwords = File.ReadAllLines("validWord.txt");
                foreach(string temp in allwords)
                {
                    tempWord = temp.Trim();
                    tempWord = tempWord.ToUpperInvariant();
                    wordList.Add(tempWord);
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show("Error loading all valid words.");
            }
        }
    
        public void generateLabels()
        {
            int width = 50;
            int height = 50;
            int spacing = 10;
            int rowWidth = width * 5 + spacing * 4;
            int x = (this.ClientSize.Width - rowWidth)/2; ///centering the labels; how far from the left
            int y = 60; /// how far down from the top

            for(int i=0; i<5; i++)
            {
                for(int j=0; j<5; j++)
                {
                    Label lblLetter = new Label();
                    lblLetter.Text = "";
                    lblLetter.Font = new Font("Rockwell", 24, FontStyle.Bold);
                    lblLetter.Size = new Size(width, height);
                    lblLetter.TextAlign = ContentAlignment.MiddleCenter;
                    lblLetter.BorderStyle = BorderStyle.FixedSingle;
                    lblLetter.BackColor = Color.Gray;
                    lblLetter.Location = new Point(x + j * (width + spacing), y);
                    lblLetter.ForeColor = Color.White;


                    this.Controls.Add(lblLetter);
                    letterLabel.Add(lblLetter);
                }
                y += 80; ///80 px between each row
            }

        }

        private void frmMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            string guess = "";
            char letter = ' ';

            if (char.IsLetter(e.KeyChar))
            {
                if (curCol < (curRow + 1) * 5)
                {
                    letter = char.ToUpper(e.KeyChar);
                    letterLabel[curCol].Text = letter.ToString();
                    curCol += 1;
                }
            }
            else if (e.KeyChar == (char)Keys.Back)
            {
                if (curCol > curRow * 5)
                {
                    curCol -= 1;
                    letterLabel[curCol].Text = "";
                }

            }
            else if (e.KeyChar == (char)(Keys.Enter))
            {
                //build the word from list of labels
                guess = buildWord();
                //MessageBox.Show("The built word is: " + guess);
                
                //check if valid word
                if (checkValid(guess))
                {
                    guesses++;
                    updateLabels(guess); //update background color

                    //check for win
                    if (checkWin(guess))
                    {
                        MessageBox.Show("You win!");
                        Application.Exit();
                        return;
                    }
                    //check to see if guesses == 6
                    else if (guesses == 6)
                    {
                        MessageBox.Show("You ran out of guesses. The word was: " + word);
                        Application.Exit();
                        return;
                    }
                    curRow++;
                    curCol = curRow * 5;

                }

                //message box for not a valid word
                else
                {
                    MessageBox.Show("Not a word");
                }
            }
        }

        public string buildWord()
        {
            string builtWord = "";
            for(int i = curRow*5; i < (curRow+1) * 5; i++)
            {
                builtWord += letterLabel[i].Text;
            }
            return builtWord;
        }

        public void updateLabels(string tempGuess)
        {
            int startIndex = 0;
            bool[] matchIndex = new bool[5];
            bool[] matchedInGuess = new bool[5]; //randul
            bool[] matchedInWord = new bool[5]; //cuv in sine

            startIndex = curRow * 5;

            for(int i = 0; i<5; i++)
            {
                if (tempGuess[i] == word[i]) //comparing letters
                {
                    letterLabel[startIndex + i].BackColor = Color.Green;
                    matchIndex[i] = true;
                    matchedInGuess[i] = true;
                    matchedInWord[i] = true;
                }
            }

            for(int i = 0; i<5; i++)
            {
                if (!matchIndex[i])
                {
                    for(int j = 0; j<5; j++)
                    {
                        if (tempGuess[i] == word[j] && !matchedInWord[j] && !matchedInGuess[i])
                        {
                            letterLabel[startIndex + i].BackColor = Color.Gold;
                            matchedInGuess[i] = true;
                            matchedInWord[j] = true;
                            break;
                        }
                    }
                }
            }
        }

        public bool checkWin(string tempGuess)
        {
            return (word == tempGuess);
        }

        public bool checkValid(string tempGuess)
        {
            return (wordList.Contains(tempGuess));
        }
    }
}
