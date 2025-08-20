# SCSClient

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 20.1.4.

## Prerequisites

Before running this project, ensure you have the following installed:

- **Node.js** (version 18 or higher)
- **Yarn** package manager

To install Yarn globally:
```bash
npm install -g yarn
```

## Environment Configuration

Configure the API endpoint in the `src/app/env.ts` file:

```typescript
const environment = {
    apiUrl: 'https://localhost:7236'  // Update this to match your backend API port (SCS.Api)
}
```

Make sure the API URL matches your backend server configuration.

## Development server

To start a local development server, run:

```bash
yarn start
# or
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Karma](https://karma-runner.github.io) test runner, use the following command:

```bash
yarn test:silent
```

## Running tests with code coverage

To generate code coverage reports along with tests:

```bash
yarn test:coverage
# or
ng test --code-coverage --browsers=ChromeHeadless --watch=false
```

The coverage report will be displayed in the terminal and generated in the `coverage/` directory.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
