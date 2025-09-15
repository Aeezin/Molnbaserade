# Check in system

A simple check in system where users submit their name.
The system is built with HTML, JS, Azure Functions, Cosmos DB and Application Insights.

## Description

The purpose of this project is to demonstrate how to

- Accept user input via a web form
- Send the data to a backend API
- Validate the input and store it in a database
- Log everyting to Application Insights
- Provide imnmediate feedback to the user

## Getting Started

### Dependencies

No installation needed.
You only need a modern web browser to use the check in form.

### Setup

**Frontend**

- Open 'index.html' localy through live server.
- Or visit the github page: https://aeezin.github.io/Molnbaserade

**Backend**

- Azure FUnction with Httptrigger.
- Writes to CosmosDB.
- Logs to Application Insights.

### Running the Project

1. Open the 'index.html' localy or visit: https://aeezin.github.io/Molnbaserade
2. Type in your name.
3. Press 'Submit'
   - If successful:
   - You will se a greeting message on the site.
   - A form is created in Cosmos DB.
   - A log entry is written to Application Insights.
