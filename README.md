# 📦 REST API: Maximum Transfer

## 📋 Problem Statement

You are given access to a paginated REST API that returns a list of patient medical transaction records. Each record represents a financial transaction (either a **credit** or **debit**) associated with a patient, including metadata like name, amount, city, and more.

Your task is to:
- **Query the paginated API**
- **Filter transactions** based on:
  - a given `userName`
  - a given `city`
- **Find the maximum credit and debit** amounts for that user in the given city
- Return both amounts as **formatted currency strings**

---

## 🌐 API Endpoint

```
GET https://jsonmock.hackerrank.com/api/transactions?page=<pageNumber>
```

### 🧾 API Response Schema

```json
{
  "page": 1,
  "per_page": 10,
  "total": 300,
  "total_pages": 30,
  "data": [
    {
      "id": "1",
      "userId": "1",
      "userName": "John Oliver",
      "txnType": "debit",
      "amount": "$1,670.57",
      "location": {
        "id": 7,
        "address": "770, Deepends, Stockton Street",
        "city": "Ripley",
        "zipCode": "44139"
      },
      "ip": "212.215.115.165"
    },
    ...
  ]
}
```

---

## 🧠 Function Specification

```csharp
Task<List<string>> transferAmount(string userName, string city)
```

### 🔧 Parameters:
- `userName` (string): Name of the user to filter on
- `city` (string): City to filter on

### 📤 Returns:
- A list of two strings:
  1. Maximum credit amount (formatted)
  2. Maximum debit amount (formatted)

---

## ✅ Example

### Sample Input

```text
userName: Bob Martin
city: Bourg
```

### Sample Output

```text
["$3,717.84", "$3,568.55"]
```

### Explanation

Filtered transactions for **Bob Martin** in **Bourg**:
```
Bob Martin (Bourg) credit $1,543.25  
Bob Martin (Bourg) credit $3,717.84  
Bob Martin (Bourg) debit $3,568.55  
Bob Martin (Bourg) debit $889.53
```

- Max credit: `$3,717.84`
- Max debit: `$3,568.55`

---

## 🛠️ Notes

- Pagination must be handled dynamically using the `total_pages` field in the response.
- The `amount` field is a string in currency format (e.g. `"$1,543.25"`) and must be parsed accordingly.
- Final result should preserve the original formatting with a **dollar sign** and **comma separators**.

---

## 💬 To Run

```sh
# Max Credit: $3,717.84 | Max Debit: $3,568.55
dotnet run --name "Bob Martin" --city "Bourg"

# Max Credit: $3,288.97 | Max Debit: $3,807.28
dotnet run --name "Helena Fernandez" --city "Ilchester"
```
