# ⚠️ This API is now classified as "legacy" and has been replaced by a REST API. As such, this library has been retired. ⚠️

# Fishbowl API Connector

Source code for accessing the Fishbowl API. Fishbowl's own source code examples are outdated and mostly irrelevant.

This API is ugly. For importing sales orders, you have to include the order details as well as any items (including shipping as an item) in the same CSV but only specify columns for the order. **Do not include column names for items.** It's a bizarre combination of JSON and CSV. Be sure to include empty value placeholders (consecutive commas) for fields that are not being imported. All columns are required and must match CSV specifications.

Check <https://www.fishbowlinventory.com/wiki/Fishbowl_API> for full command documentation. I would also recommend downloading their Fishhook application for testing new commands.

Dependent on:

- [LumenWorksCsvReader2](https://www.nuget.org/packages/LumenWorksCsvReader2/)
- [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/)

## Example Usage

### Execute Query and Return Results

```C#
string codes = "'CODE123','CODE2345'";
using (Fishbowl fishbowl = new Fishbowl("hostname.myfishbowl.com", 28192, "username", "password"))
{
    fishbowl.Connect();
    // No access to a db driver library so no parameterized queries allowed
    // BE VERY CAREFUL WHAT YOU PASS HERE
    DataTable product = fishbowl.ExecuteQuery(query: String.Format("SELECT p.id, p.num, p.price, p.partId, qi.qtyonhand, p.sku FROM product p JOIN qtyinventory qi ON p.partId = qi.partid WHERE p.sku IN ({0})", codes));
    DataTable product = fishbowl.ExecuteQuery(name: "ListProducts"); // Use saved query by name
}
```

### Import Order

```C#
List<string> order = new List<string>();
order.Add(@"""Flag"",""SONum"",""Status"",""CustomerName"",""CustomerContact"",""BillToName"",""BillToAddress"",""BillToCity"",""BillToState"",""BillToZip"",""BillToCountry"",""ShipToName"",""ShipToAddress"",""ShipToCity"",""ShipToState"",""ShipToZip"",""ShipToCountry"",""ShipToResidential"",""CarrierName"",""TaxRateName"",""PriorityId"",""PONum"",""VendorPONum"",""Date"",""Salesman"",""ShippingTerms"",""PaymentTerms"",""FOB"",""Note"",""QuickBooksClassName"",""LocationGroupName"",""FulfillmentDate"",""URL"",""CarrierService"",""DateExpired"",""Phone"",""Email""");
order.Add(@"""SO"",orderId,20...,,,");

// add items
order.Add(@"""Item"",10,""ProductSKU""...,,");

using (Fishbowl fishbowl = new Fishbowl("hostname.myfishbowl.com", 28192, "username", "password"))
{
    fishbowl.Connect();
    fish.Import("ImportSalesOrder", order);
}
```

## Known Issues

- Not all commands are implemented. Pull requests welcome!
- It's slow. It has to `Thread.Sleep()` for a minimum of 1 second between sending the request and receiving a response or the response will ***always*** be empty.
- You cannot use parameterized queries. So you have to control what gets sent or write your own SQL injection filter (or use one that someone else has already written).
