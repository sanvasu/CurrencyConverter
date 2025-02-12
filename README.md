
## Description
This project provides a currency conversion API that allows users to convert amounts between different currencies. It fetches live exchange rates from Frankfurter API and have pagination based fething results.

## Features
- Convert between currencies using the latest exchange rates.
- Caching of exchange rates for faster response times.
- Error handling for invalid inputs or failed API calls.

  ### Prerequisites
- .NET SDK 5.0 or later

  ### Steps to Run Locally

1. Clone the repository:
   ```bash
   git clone https://github.com/sanvasu/CurrencyConverter.git

** ### Assumptions Made**
- Exchange Rate API Availability: The project assumes that the external exchange rate API is available and provides valid data. Any downtime or changes in the API may affect functionality.

- Currencies Supported: The application assumes that the requested currencies are supported by the external API. If a currency is not supported, the conversion will fail.

- API Rate Limiting: There is an assumption that the API does not enforce strict rate limits. If rate limits are exceeded, the system may throttle or fail to fetch data.

- Error Handling: The system assumes that network-related errors or invalid input will be handled gracefully by throwing exceptions or returning appropriate HTTP status codes.

** ###Possible Future Enhancements**
- Support for historical exchange rates so users can convert amounts based on past exchange rates.

- Multi language support for international users to interact with their prefered language
