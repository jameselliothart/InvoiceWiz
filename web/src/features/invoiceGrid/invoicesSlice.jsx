import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";

// Async thunk to fetch invoice summaries
export const fetchInvoices = createAsyncThunk(
  "invoices/fetchInvoices",
  async (_, { rejectWithValue }) => {
    try {
      const res = await fetch("/api/invoices");
      if (!res.ok) {
        const text = await res.text().catch(() => "");
        return rejectWithValue(`HTTP ${res.status}: ${text}`);
      }
      const json = await res.json();
      return json;
    } catch (err) {
      return rejectWithValue(err.message);
    }
  }
);

const invoicesSlice = createSlice({
  name: "invoices",
  initialState: {
    items: [],
    loading: false,
    error: null,
  },
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchInvoices.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchInvoices.fulfilled, (state, action) => {
        state.loading = false;
        state.items = action.payload;
      })
      .addCase(fetchInvoices.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload || action.error.message;
      });
  },
});

export default invoicesSlice.reducer;
