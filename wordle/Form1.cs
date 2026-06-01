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

        List<Label> letterLabel = new List<Label>();
        List<Label> keyLabel = new List<Label>();

        int curCol = 0, curRow = 0;
        int guesses = 1;
        string word = "";

        HashSet<string> wordList = new HashSet<string>();
        private Dictionary<Label, Point> labelOffsets = new Dictionary<Label, Point>();
        private Point titleOffset = new Point();
        private Point btnOffset = new Point();

        bool isDarkTheme = true;

        public frmMain()
        {
            InitializeComponent();

            //add labels to letterLabel and keyLabel List
            for (int i = 1; i <= 25; i++)
            {
                Control[] found = this.Controls.Find("label" + i, false); //false - ca sa nu mearga recursiv IN continut
                letterLabel.Add((Label)found[0]);
            }

            for (char i = 'A'; i <= 'Z'; i++)
            {
                Control[] found = this.Controls.Find("keyLabel" + i, false);
                keyLabel.Add((Label)found[0]);
            }
            keyLabel.Add(labelEnter);
            keyLabel.Add(labelBack);

            //mark label offsets
            int centerX = this.ClientSize.Width / 2;
            titleOffset.X = lblTitle.Location.X;
            titleOffset.Y = lblTitle.Location.Y;
            btnOffset.X = btnTheme.Location.X;
            btnOffset.Y = btnTheme.Location.Y;
            foreach (Label lbl in letterLabel)
                labelOffsets[lbl] = new Point(lbl.Location.X - centerX, lbl.Location.Y);
            foreach (Label lbl in keyLabel)
                labelOffsets[lbl] = new Point(lbl.Location.X - centerX, lbl.Location.Y);
            
            loadWords();
            this.KeyPress += frmMain_KeyPress;
            this.KeyPreview = true;

        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            int centerX = this.ClientSize.Width / 2;

            foreach (var entry in labelOffsets)
                entry.Key.Location = new Point(entry.Value.X + centerX, entry.Value.Y);

            int modif = lblTitle.Size.Width / 2;
            lblTitle.Location = new Point(centerX - modif, lblTitle.Location.Y);

            modif = centerX - btnTheme.Size.Width - btnTheme.Location.X;
            btnTheme.Location = new Point(centerX - modif + btnTheme.Size.Width, btnTheme.Location.Y);
        }

        private void ApplyTheme()
        {
            Color background, foreground, lblBack, keyBack, title;
            if (isDarkTheme)
            {
                background = Color.Black;
                foreground = Color.White;
                lblBack = Color.DarkGray;
                keyBack = Color.DarkGray;
                title = Color.Gold;
            }
            else
            {
                background = Color.White;
                foreground = Color.Black;
                lblBack = Color.Silver;
                keyBack = Color.Silver;
                title = Color.Goldenrod;
            }

            this.BackColor = background;
            lblTitle.ForeColor = title;
            foreach (Label lbl in letterLabel)
            {
                if (lbl.BackColor == Color.DarkGray || lbl.BackColor == Color.Silver)
                    lbl.BackColor = lblBack;
                lbl.ForeColor = foreground;
            }

            foreach (Label lbl in keyLabel)
            {
                if (lbl.BackColor == Color.DarkGray || lbl.BackColor == Color.Silver)
                    lbl.BackColor = keyBack;
                lbl.ForeColor = foreground;
            }
        }

        private void btnTheme_Click(object sender, EventArgs e)
        {
            isDarkTheme = !isDarkTheme;
            ApplyTheme();
            btnTheme.Text = isDarkTheme ? "Light Mode" : "Dark Mode";
            this.ActiveControl = null; //remove focus so enter goes into guess
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
                    keyLabel[tempGuess[i] - 65].BackColor = Color.Green;
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
                            if (keyLabel[tempGuess[i] - 65].BackColor != Color.Green)
                                keyLabel[tempGuess[i] - 65].BackColor = Color.Gold;

                            matchedInGuess[i] = true;
                            matchedInWord[j] = true;
                            break;
                        }
                    }
                    if (!matchedInGuess[i]) //letter bad :(
                    {
                        letterLabel[startIndex + i].BackColor = Color.DimGray;
                        keyLabel[tempGuess[i] - 65].BackColor = Color.DimGray;
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

        // make buttons clickable, reduce latency by registering double/fast clicks as just clicks
        private void labelEnter_Click(object sender, EventArgs e)
        {
            frmMain_KeyPress(this, new KeyPressEventArgs((char)Keys.Enter));
        }
        private void labelEnter_DoubleClick(object sender, EventArgs e)
        {
            labelEnter_Click(sender, e);
        }

        private void labelBack_Click(object sender, EventArgs e)
        {
            frmMain_KeyPress(this, new KeyPressEventArgs((char)Keys.Back));
        }

        private void labelBack_DoubleClick(object sender, EventArgs e)
        {
            labelBack_Click(sender, e);
        }

        private void keyLabel_Click(object sender, EventArgs e)
        {
            Label lbl = (Label)sender; //differentiate chosen label from other labels
            char letter = lbl.Text[0];
            frmMain_KeyPress(this, new KeyPressEventArgs(letter));
        }

        private void keyLabel_DoubleClick(object sender, EventArgs e)
        {
            keyLabel_Click(sender, e);
        }
    }
}
