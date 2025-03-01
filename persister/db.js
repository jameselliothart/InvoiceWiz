const mongoose = require("mongoose");

mongoose.connect("mongodb://mongodb:27017/invoices");

const invoiceSchema = new mongoose.Schema(
  {
    _id: { type: String, required: true },
    To: { type: String, required: true },
    Amount: { type: Number, required: true },
    FileLocation: { type: String, required: false },
  },
  { _id: false }
);

exports.messageToUpdateParams = msg => {
  const params = {...msg};
  delete params.Id;
  return params;
}
exports.Invoice = mongoose.model('Invoice', invoiceSchema);