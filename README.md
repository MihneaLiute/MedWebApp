# MedWebApp

MedWebApp is a prototype Web Application designed to provide a platform for accessing and managing an on-demand medical services business.
Users can browse available services, book appointments, and manage their profiles.
The application supports different roles such as customers, providers, and administrators, each with specific functionalities.

## Features

- Browse and search for medical services
- Book and manage appointments
- Manage service provider profiles
- Admin functionalities to manage services and appointments

## Technologies Used

- ASP.NET
- Entity Framework Core
- JavaScript
- jQuery
- Bootstrap


## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

## Installation

1. **Clone the repository:**

    ```bash
    git clone https://github.com/MihneaLiute/MedWebApp.git
    cd MedWebApp
    ```

2. **Set up the database:**

    - Update the connection string in `appsettings.json` to point to your SQL Server instance.
    - Run the following commands to create and seed the database:

    ```bash
    dotnet ef database update
    ```

3. **Install client-side dependencies:**

    ```bash
    npm install
    ```

## Running the Project

1. **Build and run the application:**

    ```bash
    dotnet run
    ```

2. **Open your browser and navigate to:**

    ```
    http://localhost:5000
    ```

## Usage

- **Any User:** Browse and search available services and service packages; view their details such as descriptions, prices requirements, and disclaimers.
- **Customer Role:** Book appointments and view/manage your appointments.
- **Provider Role:** Manage your appointments and provider profile.
- **Admin Role:** Manage all services, service packages, and appointments.

## Contributing

Contributions are welcome! Please fork the repository and create a pull request with your changes.

## License

This project is licensed under the MIT License. See the `LICENSE.txt` file for more details.