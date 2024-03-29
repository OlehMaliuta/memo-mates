﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using Microsoft.Office.Interop.Word;
using MemoMates.Classes.DB;
using MemoMates.Classes.DB.Models;
using MemoMates.Tools;
using Word = Microsoft.Office.Interop.Word.Application;

namespace MemoMates
{
    public partial class ListForm : Form
    {
        public enum NextAction
        {
            AppIsClosing,
            GoToMainForm,
            Other
        }

        private readonly DbApp DB;
        private readonly PersonList reviewingList;
        private List<Person> people;
        private NextAction action;
        private int sortMode;
        private bool sortByAsc;

        public ListForm(DbApp db, PersonList pl)
        {
            InitializeComponent();

            this.DB = db;
            this.reviewingList = pl;

            this.people = db.People
                .Where(p => p.ListId == pl.Id)
                .ToList()
                .OrderBy(p => p.FullName())
                .ToList();

            this.action = NextAction.GoToMainForm;
            this.sortMode = 0;
            this.sortByAsc = true;
        }

        private void ListFormLoad(object sender, EventArgs e)
        {
            // form settings

            elementDataGridView.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            foreach (Person el in this.people)
            {
                elementDataGridView.Rows.Add
                    (el.FullName(), el.DateOfBirth, el.CreatedAt, el.UpdatedAt);
            }

            // localization

            this.Text = $"{StringTool.Get("general.app-name")} - \"{this.reviewingList.Name}\"";

            addElementButton.Text = StringTool.Get("list-form.add-element-option");
            createTxtFileButton.Text = StringTool.Get("list-form.save-txt-option");
            createDocxFileButton.Text = StringTool.Get("list-form.save-docx-option");

            elementDataGridView.Columns[0].HeaderText = StringTool.Get("list-form.column-header-1");
            elementDataGridView.Columns[1].HeaderText = StringTool.Get("list-form.column-header-2");
            elementDataGridView.Columns[2].HeaderText = StringTool.Get("list-form.column-header-3");
            elementDataGridView.Columns[3].HeaderText = StringTool.Get("list-form.column-header-4");
            elementDataGridView.Columns[4].HeaderText = StringTool.Get("list-form.column-header-5");
            elementDataGridView.Columns[5].HeaderText = StringTool.Get("list-form.column-header-6");

            searchLabel.Text = StringTool.Get("list-form.searching-option-title");

            settingsButton.Text = StringTool.Get("list-form.settings-option");
            mainMenuButton.Text = StringTool.Get("list-form.main-menu-option");
            exitButton.Text = StringTool.Get("list-form.exit-option");

            fileMenuSection.Text = StringTool.Get("list-form.top-menu-option-file");
            addElementTool.Text = StringTool.Get("list-form.add-element-option");
            createTxtTool.Text = StringTool.Get("list-form.save-txt-option");
            createDocxTool.Text = StringTool.Get("list-form.save-docx-option");
            settingsTool.Text = StringTool.Get("list-form.settings-option");
            exitTool.Text = StringTool.Get("list-form.exit-option");
        }

        private void AddElementToolClick(object sender, EventArgs e)
        {
            AddElementButtonClick(sender, e);
        }

        private void CreateTxtToolClick(object sender, EventArgs e)
        {
            CreateTxtFileButtonClick(sender, e);
        }

        private void CreateDocxToolClick(object sender, EventArgs e)
        {
            CreateDocxFileButtonClick(sender, e);
        }

        private void SettingsToolClick(object sender, EventArgs e)
        {
            SettingsButtonClick(sender, e);
        }

        private void ExitToolClick(object sender, EventArgs e)
        {
            ExitButtonClick(sender, e);
        }

        private void AddElementButtonClick(object sender, EventArgs e)
        {
            this.action = NextAction.Other;
            this.Close();
            ElementForm elementForm = new ElementForm(this.DB, this.reviewingList, null);
            elementForm.Show();
        }

        private void SettingsButtonClick(object sender, EventArgs e)
        {
            this.action = NextAction.Other;
            this.Close();
            SettingsForm settingsForm = new SettingsForm(this.DB, this.reviewingList, SettingsForm.PrevForm.ListForm);
            settingsForm.Show();
        }

        private void MainMenuButtonClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ExitButtonClick(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                                StringTool.Get("general.exit-message"),
                                StringTool.Get("general.warning-message-title"),
                                MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                this.action = NextAction.AppIsClosing;
                this.Close();
            }
        }

        private void ElementDataGridViewCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                switch (elementDataGridView.Columns[e.ColumnIndex].Name)
                {
                    case "open":
                        {
                            this.action = NextAction.Other;
                            this.Close();

                            ElementForm elementForm = new ElementForm(
                                    this.DB,
                                    this.reviewingList,
                                    this.people[e.RowIndex]);

                            elementForm.Show();
                        }
                        break;
                    case "delete":
                        {
                            DialogResult result = MessageBox.Show(
                                StringTool.Get("list-form.remove-element-message"),
                                StringTool.Get("general.warning-message-title"),
                                MessageBoxButtons.YesNo);

                            if (result == DialogResult.Yes)
                            {
                                this.DB.People.Remove(this.people[e.RowIndex]);
                                this.people.Remove(this.people[e.RowIndex]);
                                this.elementDataGridView.Rows.RemoveAt(e.RowIndex);

                                this.DB.SaveChanges();
                            }
                        }
                        break;
                }
            }
        }

        private void CreateTxtFileButtonClick(object sender, EventArgs e)
        {
            p_a:

            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "All files (*.*)|*.*";
            fileDialog.InitialDirectory = @"C:\Users\User\Downloads";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                if (fileDialog.FileName.Contains('.'))
                {
                    MessageBox.Show(
                        StringTool.Get("list-form.illegal-chars-message"), 
                        StringTool.Get("general.warning-message-title"));
                    goto p_a;
                }

                string listData;
                listData = "\"" + this.reviewingList.Name + "\"\n\n";
                for (int i = 0; i < this.people.Count; i++)
                {
                    listData += $"\n{i + 1}. {this.people[i].FullName()}\n";
                    List<string> fields = new List<string>
                    {
                        StringTool.Get("list-form.fields-for-documents-1") +
                        this.people[i].DateOfBirth,
                        StringTool.Get("list-form.fields-for-documents-2") +
                        this.people[i].PhoneNumber,
                        StringTool.Get("list-form.fields-for-documents-3") +
                        this.people[i].EmailAddress,
                        StringTool.Get("list-form.fields-for-documents-4") +
                        this.people[i].ExtraInfo,
                        StringTool.Get("list-form.fields-for-documents-5") +
                        this.people[i].CreatedAt,
                        StringTool.Get("list-form.fields-for-documents-6") +
                        this.people[i].UpdatedAt
                    };

                    for (int j = 0; j < fields.Count; j++)
                    {
                        listData += "\n" + fields[j] + "\n";
                    }
                    listData += "\n";
                }

                File.WriteAllText(fileDialog.FileName + ".txt", listData);

                MessageBox.Show(
                    StringTool.Get("list-form.document-created-message"), 
                    StringTool.Get("general.normal-message-title"));
            }
        }

        private void CreateDocxFileButtonClick(object sender, EventArgs e)
        {
            p_b:

            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "All files (*.*)|*.*";
            fileDialog.InitialDirectory = @"C:\Users\User\Downloads";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                if (fileDialog.FileName.Contains('.'))
                {
                    MessageBox.Show(
                        StringTool.Get("list-form.illegal-chars-message"),
                        StringTool.Get("general.warning-message-title"));
                    goto p_b;
                }

                Word app = new Word();
                Document docx = app.Documents.Add(Visible: false);
                Range r = docx.Range();

                r.Text = "\"" + this.reviewingList.Name + "\"\n\n";
                for (int i = 0; i < this.people.Count; i++)
                {
                    r.Text += $"\n{i + 1}. {this.people[i].FullName()}\n";
                    List<string> fields = new List<string>
                    {
                        StringTool.Get("list-form.fields-for-documents-1") +
                        this.people[i].DateOfBirth,
                        StringTool.Get("list-form.fields-for-documents-2") +
                        this.people[i].PhoneNumber,
                        StringTool.Get("list-form.fields-for-documents-3") +
                        this.people[i].EmailAddress,
                        StringTool.Get("list-form.fields-for-documents-4") +
                        this.people[i].ExtraInfo,
                        StringTool.Get("list-form.fields-for-documents-5") +
                        this.people[i].CreatedAt,
                        StringTool.Get("list-form.fields-for-documents-6") +
                        this.people[i].UpdatedAt
                    };

                    for (int j = 0; j < fields.Count; j++)
                    {
                        r.Text += "\n" + fields[j] + "\n";
                    }
                    r.Text += "\n";
                }

                docx.SaveAs(fileDialog.FileName + ".docx");
                docx.Close();
                app.Quit();

                MessageBox.Show(
                    StringTool.Get("list-form.document-created-message"),
                    StringTool.Get("general.normal-message-title"));
            }
        }

        private void SearchElementTextBoxTextChanged(object sender, EventArgs e)
        {
            this.elementDataGridView.Rows.Clear();

            foreach (var el in this.people)
            {
                if (el.FullName().ToLower().Contains(this.searchElementTextBox.Text.ToLower()))
                {
                    this.elementDataGridView.Rows.Add(el.FullName(), el.DateOfBirth, el.CreatedAt, el.UpdatedAt);
                }
            }
        }

        private void ElementDataGridViewColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex > 3)
            {
                return;
            }

            this.sortByAsc = sortMode == e.ColumnIndex ? !this.sortByAsc : true;
            this.sortMode = e.ColumnIndex;

            if (this.sortByAsc)
            {
                switch (this.sortMode)
                {
                    case 0:
                        this.people = this.people.OrderBy(p => p.FullName()).ToList();
                        break;
                    case 1:
                        this.people = this.people.OrderBy(p => p.DateOfBirth).ToList();
                        break;
                    case 2:
                        this.people = this.people.OrderBy(p => p.CreatedAt).ToList();
                        break;
                    case 3:
                        this.people = this.people.OrderBy(p => p.UpdatedAt).ToList();
                        break;
                }
            }
            else
            {
                switch (this.sortMode)
                {
                    case 0:
                        this.people = this.people.OrderByDescending(p => p.FullName()).ToList();
                        break;
                    case 1:
                        this.people = this.people.OrderByDescending(p => p.DateOfBirth).ToList();
                        break;
                    case 2:
                        this.people = this.people.OrderByDescending(p => p.CreatedAt).ToList();
                        break;
                    case 3:
                        this.people = this.people.OrderByDescending(p => p.UpdatedAt).ToList();
                        break;
                }
            }

            this.elementDataGridView.Rows.Clear();

            foreach (var el in this.people)
            {
                if (el.FullName().ToLower().Contains(this.searchElementTextBox.Text.ToLower()))
                {
                    this.elementDataGridView.Rows.Add(el.FullName(), el.DateOfBirth, el.CreatedAt, el.UpdatedAt);
                }
            }
        }

        private void ListFormFormClosing(object sender, FormClosingEventArgs e)
        {
            switch (this.action)
            {
                case NextAction.AppIsClosing: 
                    Environment.Exit(0); 
                    break;
                case NextAction.GoToMainForm:
                    MainForm mainForm = new MainForm(this.DB);
                    mainForm.Show();
                    break;
            }
        }
    }
}
