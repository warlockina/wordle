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

/* NOTES *
 * 400x500 original window size jsyk
 * 
 * 
 * 
*/
namespace wordle
{
    public partial class frmMain : Form
    {

        List<Label> letterLabel = new List<Label>();
        int curCol = 0, curRow = 0;
        int guesses = 1;
        string word = "";
        HashSet<string> wordList = new HashSet<string>();
        private Dictionary<Label, Point> labelOffsets = new Dictionary<Label, Point>();
        private Point titleOffset = new Point();

        public frmMain()
        {
            InitializeComponent();

            //add labels to letterLabel List
            for(int i=1; i<=25; i++)
            {
                Control[] found = this.Controls.Find("label" + i, false); //false - ca sa nu mearga recursiv IN continut
                letterLabel.Add((Label)found[0]);
            }

            //
            int centerX = this.ClientSize.Width / 2;
            titleOffset.X = lblTitle.Location.X;
            titleOffset.Y = lblTitle.Location.Y;
            foreach (Label lbl in letterLabel)
                labelOffsets[lbl] = new Point(lbl.Location.X - centerX, lbl.Location.Y);

            ///////
            //generateLabels();
            loadWords();
            this.KeyPress += frmMain_KeyPress;
            this.KeyPreview = true;

        }

        private void frmMain_Resize(object sender, EventArgs e)
        { //vezi cu centratul casutelor
            int centerX = this.ClientSize.Width / 2;
            foreach (Label lbl in letterLabel) //(-10) pt SPACING
                lbl.Location = new Point(labelOffsets[lbl].X + centerX, labelOffsets[lbl].Y);

            int modif = lblTitle.Size.Width / 2;
            lblTitle.Location = new Point(centerX - modif, lblTitle.Location.Y);
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
                foreach (string temp in allwords)
                {
                    tempWord = temp.Trim(); //remove whitespaces
                    tempWord = tempWord.ToUpperInvariant(); //for diff keyboards
                    wordList.Add(tempWord);
                }
            }
            catch (IOException ex) //daca nu convine
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
                foreach (string temp in allwords)
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

                if (checkValid(guess))
                {
                    guesses++;
                    updateLabels(guess); //background color

                    if (checkWin(guess))
                    {
                        MessageBox.Show("You win!");
                        Application.Exit();
                        return;
                    }
                    //check to see if there's a loss
                    else if (guesses == 6)
                    {
                        MessageBox.Show("You ran out of guesses. The word was: " + word);
                        Application.Exit();
                        return;
                    }

                    //NEXT !!
                    curRow++;
                    curCol = curRow * 5;
                }
                else
                {
                    MessageBox.Show("Not a word");
                }
            }
        }

        public string buildWord()
        {
            string builtWord = "";
            for (int i = curRow * 5; i < (curRow + 1) * 5; i++)
            {
                builtWord += letterLabel[i].Text;
            }
            return builtWord;
        }

        public void updateLabels(string tempGuess)
        {
            int startIndex = 0;
            bool[] matchIndex = new bool[5]; //if they're green
            bool[] matchedInGuess = new bool[5]; //matched inside the guessword
            bool[] matchedInWord = new bool[5]; //prevent from unnecessary yellows

            startIndex = curRow * 5;

            for (int i = 0; i < 5; i++)
            {
                if (tempGuess[i] == word[i]) //comparing letters
                {
                    //letter is good :), pos is good :)
                    letterLabel[startIndex + i].BackColor = Color.Green;
                    matchIndex[i] = true;
                    matchedInGuess[i] = true;
                    matchedInWord[i] = true;
                }
            }

            for (int i = 0; i < 5; i++)
            {
                if (!matchIndex[i]) //daca litera resp nu e verde/daca n am gasit
                {
                    for (int j = 0; j < 5; j++) //ma uit daca e litera in cuvant
                    {
                        if (tempGuess[i] == word[j] && !matchedInWord[j] && !matchedInGuess[i])
                        {
                            //letter is good :), pos is bad :(
                            letterLabel[startIndex + i].BackColor = Color.Gold;
                            matchedInGuess[i] = true;
                            matchedInWord[j] = true;
                            break;
                        }
                    }
                    if (!matchedInGuess[i]) //letter bad :(
                    {
                        letterLabel[startIndex + i].BackColor = Color.DimGray;
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

        public void generateLabels()
        {
            int width = 50;
            int height = 50;
            int spacing = 10;
            int rowWidth = width * 5 + spacing * 4;

            int x = (this.ClientSize.Width - rowWidth) / 2; ///centering the labels; how far from the left
            int y = 60; /// how far down from the top

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Label lblLetter = new Label();
                    lblLetter.Text = "";
                    lblLetter.Font = new Font("Arial", 24, FontStyle.Bold);
                    lblLetter.Size = new Size(width, height);
                    lblLetter.TextAlign = ContentAlignment.MiddleCenter;
                    lblLetter.BorderStyle = BorderStyle.FixedSingle;
                    lblLetter.BackColor = Color.DarkGray;
                    lblLetter.Location = new Point(x + j * (width + spacing), y);
                    lblLetter.ForeColor = Color.White;


                    this.Controls.Add(lblLetter);
                    letterLabel.Add(lblLetter);
                }
                y += 80; ///80 px between each row
            }

        }

    }
}
