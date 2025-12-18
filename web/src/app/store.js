import { configureStore } from "@reduxjs/toolkit";
import downloadUrlReducer from "../features/invoiceForm/downloadUrlSlice";
import invoiceReducer from "../features/invoiceForm/invoiceSlice";
import invoicesReducer from "../features/invoiceGrid/invoicesSlice";

export const store = configureStore({
  reducer: {
    downloadUrl: downloadUrlReducer,
    invoice: invoiceReducer,
    invoices: invoicesReducer,
  },
});
