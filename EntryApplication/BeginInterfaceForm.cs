﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Sql;
using System.Data.SqlClient;
using System.ServiceProcess;
using System.Data.SqlTypes;


namespace EntryApplication
{
    // Data about a patron
    public struct Patron
    {
        public string firstName,
            lastName,
            middleInitial,
            gender,
            family,
            dateOfBirth,

            phoneNumber,
            address,
            comments;
    }

    public partial class BeginInterfaceForm : Form
    {
        #region Constants for Querying SQL Columns
        // Hardcoded strings for all of the column names in SQL
        private const string patronFirstName = "FirstName";
        private const string patronLastName = "LastName";
        private const string patronMiddleInitial = "MiddleInitial";
        private const string patronGender = "Gender";
        private const string patronFamily = "Family";
        private const string patronDateOfLastVisit = "LastVisit";
        private const string patronDateOfBirth = "DateOfBirth";
        private const string patronAddress = "Address";
        private const string patronPhoneNumber = "PhoneNumber";
        private const string patronComments = "Comments";
        private const string patronInitialVisitDate = "InitialVisitDate";
        #endregion

        // The type of date we want to display: mm/dd/yy
        private const string dateCode = "d";

        // The date
        private string date;

        // The connection to the local sql database
        private SqlConnection sqlConnection;

        public BeginInterfaceForm()
        {
            InitializeComponent();

            // Manual initialization
            outputDataView.Columns[4].SortMode = DataGridViewColumnSortMode.NotSortable;
            outputDataView.Columns[5].SortMode = DataGridViewColumnSortMode.NotSortable;
            outputDataView.Columns[6].SortMode = DataGridViewColumnSortMode.NotSortable;

            // Setup the dataview
            outputDataView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Show the date on the datemessage
            DateTime today = DateTime.Today;
            date = today.ToString(dateCode);
            dateLabel.Text = "Today's Date is: " + date;

            // Connect to the local SQL Database
            sqlConnection = new SqlConnection("Server=localhost\\SQLEXPRESS;Database=Patrons;User Id=sa; Password=potato");
            sqlConnection.Open();

            this.WindowState = FormWindowState.Maximized;

            LoadAllPatrons();
        }

        // A second constructor, currently unused
        private void BeginInterfaceForm_Load(object sender, EventArgs e)
        {

        }

        // Load all patrons
        private void LoadAllPatrons()
        {
            outputDataView.Rows.Clear();

            // Initialize all of the patrons into the list
            SqlCommand patronsCommand = new SqlCommand("SELECT * FROM Patrons;", sqlConnection);
            SqlDataReader patrons = null;

            // Read all of the patrons into the data
            patrons = patronsCommand.ExecuteReader();
            while (patrons.Read())
            {
                AddDataRow(patrons[patronFirstName].ToString(),
                    patrons[patronMiddleInitial].ToString(),
                    patrons[patronLastName].ToString(),
                    patrons[patronGender].ToString(),
                    patrons[patronDateOfLastVisit].ToString(),
                    patrons[patronDateOfBirth].ToString(),
                    patrons[patronFamily]);
            }
            patrons.Close();
        }

        // Shorthand for adding a set of values to the outputDataView
        private void AddDataRow(params object[] values)
        {
            this.outputDataView.Rows.Add(values);
        }

        // Whenever a letter is added to or removed from the search box
        private void searchBoxChanged(object sender, EventArgs e)
        {
            if (searchBox.Text.Length>2)
            {
                UpdateResults(searchBox.Text);
            }
        }

        // Whenever a key is pressed in the search box
        private void searchBoxKeyDown(object sender, KeyEventArgs e)
        {
            // Highlight everything if a space was entered. No spaces in last names
            if (e.KeyCode == Keys.Space)
            {
                e.SuppressKeyPress = true;
                LoadAllPatrons();

                if (!String.IsNullOrEmpty(searchBox.Text))
                {
                    searchBox.SelectionStart = 0;
                    searchBox.SelectionLength = searchBox.Text.Length;
                }
            }
        }

        // Update the data table with the results given the search box text
        private void UpdateResults(string query)
        {
            SqlCommand searchCommand = new SqlCommand("SELECT * FROM Patrons WHERE FirstName LIKE '" + query + "%' OR LastName LIKE '" + query + "%' OR Family LIKE '%" + query + "%'", sqlConnection);

            SqlDataReader results = searchCommand.ExecuteReader();

            outputDataView.Rows.Clear();
            while (results.Read())
            {
                AddDataRow(results[patronFirstName].ToString(),
                    results[patronMiddleInitial].ToString(),
                    results[patronLastName].ToString(),
                    results[patronGender].ToString(),
                    results[patronDateOfLastVisit].ToString(),
                    results[patronDateOfBirth].ToString(),
                    results[patronFamily].ToString());
            }
            results.Close();
        }

        // When the button to enter a new patron is clicked
        private void addPatronButtonClick(object sender, EventArgs e)
        {
            NewPatronForm form = new NewPatronForm();
            form.ShowDialog();

            if (form.Saved())
            {
                Patron p = form.GetData();
                string data = "'" + p.firstName + "','"
                    + p.middleInitial + "','"
                    + p.lastName + "','"
                    + p.gender + "','"
                    + "','"
                    + p.dateOfBirth + "','"
                    + p.family + "','"
                    + p.phoneNumber + "','"
                    + p.address + "','"
                    + p.comments + "','"
                    + date + "'";


                // Add in the values
                SqlCommand addCommand = new SqlCommand("INSERT INTO Patrons VALUES (" + data + ")", sqlConnection);
                addCommand.ExecuteNonQuery();
            }
        }

        // When the button to edit an existing patron is clicked
        private void editPatronButtonClick(object sender, EventArgs e)
        {
            DataGridViewRow row = outputDataView.SelectedRows[0];

            // Load existing data about patron

            string queryCommandString = "SELECT * FROM PATRONS WHERE "
                + patronFirstName + "='" + row.Cells[0].Value.ToString() + "' AND "
                + patronLastName + "='" + row.Cells[2].Value.ToString() + "' AND "
                + patronFamily + "='" + row.Cells[6].Value.ToString() + "'";

            SqlCommand searchCommand = new SqlCommand(queryCommandString, sqlConnection);
            SqlDataReader search = searchCommand.ExecuteReader();
            search.Read();

            string firstName, lastName, middleInitial, gender, lastVisit, dateOfBirth, family, address, phoneNumber, comments, initialVisitDate;
            firstName = search[patronFirstName].ToString();
            lastName = search[patronLastName].ToString();
            middleInitial = search[patronMiddleInitial].ToString();
            gender = search[patronGender].ToString();
            lastVisit = search[patronDateOfLastVisit].ToString();
            dateOfBirth = search[patronDateOfBirth].ToString();
            family = search[patronFamily].ToString();
            address = search[patronAddress].ToString();
            phoneNumber = search[patronPhoneNumber].ToString();
            comments = search[patronComments].ToString();
            initialVisitDate = search[patronInitialVisitDate].ToString();
            search.Close();
            

            // Get new data
            NewPatronForm form = new NewPatronForm(firstName, lastName, middleInitial, gender, dateOfBirth, family, address, phoneNumber, comments);
            form.ShowDialog();

            if (form.Saved())
            {
                Patron p = form.GetData();
                string data = patronFirstName + "='" + p.firstName + "'" + ','
                    + patronMiddleInitial + "='" + p.middleInitial + "'" + ','
                    + patronLastName + "='" + p.lastName + "'" + ','
                    + patronGender + "='" + p.gender + "'" + ','
                    + patronDateOfLastVisit + "='" + lastVisit + "'" + ','
                    + patronDateOfBirth + "='" + p.dateOfBirth + "'" + ','
                    + patronFamily + "='" + p.family + "'" + ','
                    + patronPhoneNumber + "='" + p.phoneNumber + "'" + ','
                    + patronAddress + "='" + p.address + "'" + ','
                    + patronComments + "='" + p.comments + "'" + ','
                    + patronInitialVisitDate + "='" + initialVisitDate + "'";

                // Remove old values and replace with new ones
                string command = "UPDATE Patrons SET " + data + " WHERE "
                + patronFirstName + "='" + row.Cells[0].Value.ToString() + "' AND "
                + patronLastName + "='" + row.Cells[2].Value.ToString() + "' AND "
                + patronFamily + "='" + row.Cells[6].Value.ToString() + "'";

                SqlCommand updateCommand = new SqlCommand(command, sqlConnection);
                updateCommand.ExecuteNonQuery();
            }

            search.Close();
        }

        // When the button to print a report is clicked
        private void printVisitButtonClick(object sender, EventArgs e)
        {
            DataGridViewRow row = outputDataView.SelectedRows[0];

            string firstName = row.Cells[0].Value.ToString();
            string lastName = row.Cells[2].Value.ToString();
            string middleInitial = row.Cells[1].Value.ToString();

            int portionsAllowed;
            // Calculate the amount of portions allowed
            if (row.Cells[5].Value.ToString()!="")
            {
                // Is a child.
                portionsAllowed = 0;
            }
            else
            {
                // Get the number of children
                int providees = row.Cells[6].Value.ToString().Split(',').Length;

                // Check for a spouse
                if (row.Cells[7].Value.ToString() != "")
                    providees++;

                // 1 Limit
                if (providees <= 3)
                    portionsAllowed = 1;

                // 2 Limits
                else if (providees <= 5)
                    portionsAllowed = 2;

                // 3 Limits
                else
                    portionsAllowed = 3;
            }
            DateTime today = DateTime.Today;

            // Show the form
            PrintVisitForm printForm = new PrintVisitForm(firstName, middleInitial, lastName, portionsAllowed, today.ToString(dateCode));
            printForm.ShowDialog();
        }

        // When the button to view more information about a patron is clicked\
        private void morePatronInfoButtonClick(object sender, EventArgs e)
        {
            DataGridViewRow row = outputDataView.SelectedRows[0];

            string firstName = row.Cells[0].Value.ToString();
            string lastName = row.Cells[2].Value.ToString();
            string middleInitial = row.Cells[1].Value.ToString();

            string name = firstName + ' ' + middleInitial + ' ' + lastName;

            string queryCommandString = "SELECT * FROM PATRONS WHERE "
                + patronFirstName + "='" + row.Cells[0].Value.ToString() +"' AND "
                + patronLastName + "='" + row.Cells[2].Value.ToString() +"' AND "
                + patronFamily + "='" + row.Cells[6].Value.ToString() + "'";

            SqlCommand queryCommand = new SqlCommand(queryCommandString, sqlConnection);
            SqlDataReader query = queryCommand.ExecuteReader();
            query.Read();

            string dateOfBirth = query[patronDateOfBirth].ToString(); ;
            string address = query[patronAddress].ToString();
            string phoneNumber = query[patronPhoneNumber].ToString();
            string lastVisit = query[patronDateOfLastVisit].ToString();
            string firstVisit = query[patronInitialVisitDate].ToString();
            string family = query[patronFamily].ToString();
            string comments = query[patronComments].ToString();

            query.Close();

            MoreInfoForm form = new MoreInfoForm(name, dateOfBirth, address, phoneNumber, lastVisit, firstVisit, family, comments);
            form.ShowDialog();
        }
    }
}
