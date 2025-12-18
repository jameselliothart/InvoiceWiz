import viteLogo from "/vite.svg";
import "./App.css";
import InvoiceForm from "./features/invoiceForm/InvoiceForm";
import InvoiceGrid from "./features/invoiceGrid/InvoiceGrid";

function App() {
  return (
    <div className="container mt-4">
      <div className="row justify-content-center">
        <div className="col-12 text-center mb-3">
          <a href="https://vite.dev" target="_blank" rel="noopener noreferrer">
            <img src={viteLogo} className="logo" alt="Vite logo" />
          </a>
        </div>
        <div className="col-md-8">
          <h1 className="text-center">Welcome to Invoice Wizard</h1>
          <InvoiceForm />
          <InvoiceGrid />
        </div>
      </div>
    </div>
  );
}

export default App;
