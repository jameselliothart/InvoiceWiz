import { createSlice } from "@reduxjs/toolkit";

const invoiceSlice = createSlice({
  name: "invoice",
  initialState: { currentId: null },
  reducers: {
    setCurrentInvoiceId: (state, action) => {
      state.currentId = action.payload;
    },
    resetCurrentInvoiceId: (state) => {
      state.currentId = null;
    },
  },
});

export const { setCurrentInvoiceId, resetCurrentInvoiceId } =
  invoiceSlice.actions;
export default invoiceSlice.reducer;
