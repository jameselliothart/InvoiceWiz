import React from "react";
import ReactDOM from "react-dom/client";
import { Provider } from "react-redux";
import { store } from "./app/store";
import App from "./App";
import SignalRProvider from "./features/signalr/SignalRProvider";

ReactDOM.createRoot(document.getElementById("root")).render(
  <React.StrictMode>
    <Provider store={store}>
      <SignalRProvider>
        <App />
      </SignalRProvider>
    </Provider>
  </React.StrictMode>
);
