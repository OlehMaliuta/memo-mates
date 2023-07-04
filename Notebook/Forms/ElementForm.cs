﻿using System;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using Notebook.Classes;

namespace Notebook
{
    public partial class ElementForm : Form
    {
        private ListsStorage listsStorage = new ListsStorage();
        private ProgVarStorage progVarStorage = new ProgVarStorage();
        private PeopleList reviewList = new PeopleList();
        private Element reviewElement = new Element();
        private Element newElement = new Element();
        private string nextWindow = "";

        public ElementForm()
        {
            InitializeComponent();
        }

        private void ElementFormLoad(object sender, EventArgs e)
        {
            // form settings

            this.listsStorage =
                JsonConvert.DeserializeObject<ListsStorage>(
                    File.ReadAllText("ListsStorageInfo.json"));

            this.progVarStorage =
                JsonConvert.DeserializeObject<ProgVarStorage>(
                    File.ReadAllText("ProgVarStorageInfo.json"));

            this.reviewList = listsStorage.PeopleLists.Single(
                        item => 
                        item.ListName == progVarStorage.ReviewListName);

            if (progVarStorage.ElementFormVariant == "change")
            {
                reviewElement = reviewList.Elements.Single(
                    p => p.Name == progVarStorage.ReviewElementName);

                newElement.CopyElement(reviewElement);
            }

            fieldNameLabel.Text = infoFieldTypeComboBox.Text;
            infoTextBox.Text = newElement.Name;

            dayNumericUpDown.Maximum = int.MaxValue;
            dayNumericUpDown.Minimum = int.MinValue;

            monthNumericUpDown.Maximum = int.MaxValue;
            monthNumericUpDown.Minimum = int.MinValue;

            yearNumericUpDown.Maximum = int.MaxValue;
            yearNumericUpDown.Minimum = int.MinValue;

            datePanel.Visible = false;

            // localization

            this.Text = progVarStorage.ElementFormVariant == "create"
                ? $"{Locale.Get("general.app-name")} - {Locale.Get("element-form.form-name-create")}"
                : $"{Locale.Get("general.app-name")} - {Locale.Get("element-form.form-name-review")}";

            editElementButton.Text = progVarStorage.ElementFormVariant == "create"
                ? Locale.Get("element-form.create-element-button")
                : Locale.Get("element-form.edit-element-button");

            dayLabel.Text = Locale.Get("element-form.day-label");
            monthLabel.Text = Locale.Get("element-form.month-label");
            yearLabel.Text = Locale.Get("element-form.year-label");

            goBackButton.Text = Locale.Get("element-form.go-back-button");

            fieldTypeLabel.Text = Locale.Get("element-form.field-title");

            infoFieldTypeComboBox.Items.Add(Locale.Get("element-form.field-type-1"));
            infoFieldTypeComboBox.Items.Add(Locale.Get("element-form.field-type-2"));
            infoFieldTypeComboBox.Items.Add(Locale.Get("element-form.field-type-3"));
            infoFieldTypeComboBox.Items.Add(Locale.Get("element-form.field-type-4"));
            infoFieldTypeComboBox.Items.Add(Locale.Get("element-form.field-type-5"));
            infoFieldTypeComboBox.Items.Add(Locale.Get("element-form.field-type-6"));
            infoFieldTypeComboBox.Items.Add(Locale.Get("element-form.field-type-7"));
            infoFieldTypeComboBox.Items.Add(Locale.Get("element-form.field-type-8"));
            infoFieldTypeComboBox.Items.Add(Locale.Get("element-form.field-type-9"));
            infoFieldTypeComboBox.Items.Add(Locale.Get("element-form.field-type-10"));

            infoFieldTypeComboBox.SelectedIndex = 0;
        }

        private void ChangeElementButtonClick(object sender, EventArgs e)
        {
            string err = "";

            if (
                progVarStorage.ElementFormVariant == "create" &&
                reviewList.Elements.FindIndex(
                    item => item.Name == infoTextBox.Text) != -1
                )
                err += "\n" + Locale.Get("element-form.same-name-message");

            if (newElement.Name == "")
                err += "\n" + Locale.Get("element-form.must-have-name-message");

            if (
                dayNumericUpDown.Value < 0 ||
                monthNumericUpDown.Value < 0 ||
                yearNumericUpDown.Value < 0)
                err += "\n" + Locale.Get("element-form.positive-values-only-message");

            if (err != "")
            {
                MessageBox.Show(err, Locale.Get("general.warning-message-title"));
                return;
            }

            string day, month, year;

            if (dayNumericUpDown.Value == 0)
                day = "-";
            else
                day = dayNumericUpDown.Value.ToString();

            if (monthNumericUpDown.Value == 0)
                month = "-";
            else
                month = monthNumericUpDown.Value.ToString();

            if (yearNumericUpDown.Value == 0)
                year = "-";
            else
                year = yearNumericUpDown.Value.ToString();

            newElement.Birthday =
                    $"{day}.{month}.{year}";

            if (progVarStorage.ElementFormVariant == "create")
            {
                newElement.CreatingDate =
                    DateTime.Now.ToShortDateString() +
                    "\n" + DateTime.Now.ToLongTimeString();

                newElement.UpdatingDate =
                    DateTime.Now.ToShortDateString() +
                    "\n" + DateTime.Now.ToLongTimeString();

                reviewList.UpdatingDate =
                    DateTime.Now.ToShortDateString() +
                    "\n" + DateTime.Now.ToLongTimeString();

                reviewList.Elements.Add(newElement);
            }
            else if (progVarStorage.ElementFormVariant == "change")
            {
                if (newElement != reviewElement)
                {
                    newElement.UpdatingDate =
                    DateTime.Now.ToShortDateString() +
                    "\n" + DateTime.Now.ToLongTimeString();

                    reviewList.UpdatingDate =
                    DateTime.Now.ToShortDateString() +
                    "\n" + DateTime.Now.ToLongTimeString();

                    reviewElement.CopyElement(newElement);
                }
            }

            nextWindow = "ListForm";
            this.Close();
        }

        private void GoBackButtonClick(object sender, EventArgs e)
        {
            nextWindow = "ListForm";
            this.Close();
        }

        private void InfoTextBoxTextChanged(object sender, EventArgs e)
        {
            switch (infoFieldTypeComboBox.SelectedIndex)
            {
                case 0:
                    newElement.Name = infoTextBox.Text;
                    break;

                case 2:
                    newElement.Phone = infoTextBox.Text;
                    break;

                case 3:
                    newElement.PersonalData = infoTextBox.Text;
                    break;

                case 4:
                    newElement.ResidentialAddress = infoTextBox.Text;
                    break;

                case 5:
                    newElement.Locale = infoTextBox.Text;
                    break;

                case 6:
                    newElement.FirstMeeting = infoTextBox.Text;
                    break;

                case 7:
                    newElement.FamilarPeoplePosition = infoTextBox.Text;
                    break;

                case 8:
                    newElement.Skills = infoTextBox.Text;
                    break;

                case 9:
                    newElement.ExtraInfo = infoTextBox.Text;
                    break;
            }
        }

        private void InfoFieldTypeComboBoxSelectedIndexChanged(
            object sender, EventArgs e)
        {
            fieldNameLabel.Text = infoFieldTypeComboBox.Text;

            switch (infoFieldTypeComboBox.SelectedIndex)
            {
                case 0:
                    infoTextBox.Visible = true;
                    datePanel.Visible = false;
                    infoTextBox.Text = newElement.Name;
                    break;

                case 1:
                    infoTextBox.Visible = false;
                    datePanel.Visible = true;
                    string[] date = newElement.Birthday.Split('.');
                    
                    if (date[0] == "-")
                        dayNumericUpDown.Value = 0;
                    else
                        dayNumericUpDown.Value = Convert.ToDecimal(date[0]);

                    if (date[1] == "-")
                        monthNumericUpDown.Value = 0;
                    else
                        monthNumericUpDown.Value = Convert.ToDecimal(date[1]);

                    if (date[2] == "-")
                        yearNumericUpDown.Value = 0;
                    else
                        yearNumericUpDown.Value = Convert.ToDecimal(date[2]);
                    break;

                case 2:
                    infoTextBox.Visible = true;
                    datePanel.Visible = false;
                    infoTextBox.Text = newElement.Phone;
                    break;

                case 3:
                    infoTextBox.Visible = true;
                    datePanel.Visible = false;
                    infoTextBox.Text = newElement.PersonalData;
                    break;

                case 4:
                    infoTextBox.Visible = true;
                    datePanel.Visible = false;
                    infoTextBox.Text = newElement.ResidentialAddress;
                    break;

                case 5:
                    infoTextBox.Visible = true;
                    datePanel.Visible = false;
                    infoTextBox.Text = newElement.Locale;
                    break;

                case 6:
                    infoTextBox.Visible = true;
                    datePanel.Visible = false;
                    infoTextBox.Text = newElement.FirstMeeting;
                    break;

                case 7:
                    infoTextBox.Visible = true;
                    datePanel.Visible = false;
                    infoTextBox.Text = newElement.FamilarPeoplePosition;
                    break;

                case 8:
                    infoTextBox.Visible = true;
                    datePanel.Visible = false;
                    infoTextBox.Text = newElement.Skills;
                    break;

                case 9:
                    infoTextBox.Visible = true;
                    datePanel.Visible = false;
                    infoTextBox.Text = newElement.ExtraInfo;
                    break;
            }
        }

        private void ElementFormFormClosing(
            object sender, FormClosingEventArgs e)
        {
            File.WriteAllText(
                "ListsStorageInfo.json",
                JsonConvert.SerializeObject(this.listsStorage));

            File.WriteAllText(
                "ProgVarStorageInfo.json",
                JsonConvert.SerializeObject(this.progVarStorage));

            switch (nextWindow)
            {
                case "ListForm":
                    ListForm listForm1 = new ListForm();
                    listForm1.Show();
                    break;

                case "":
                    ListForm listForm2 = new ListForm();
                    listForm2.Show();
                    break;

                default:
                    MainForm defForm = new MainForm();
                    defForm.Show();
                    MessageBox.Show("ERROR");
                    break;
            }
        }
    }
}
