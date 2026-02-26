# Manager Transactions Table - Angular Integration Guide

## 📋 Overview

This guide provides complete instructions for integrating the **ManagerTransactionsController** backend with your Angular transaction table component.

---

## 🎯 Backend Controller Features

The `ManagerTransactionsController` provides:

### Endpoints:
1. **GET `/api/manager-transactions`** - Get paginated, filtered transactions
2. **GET `/api/manager-transactions/high-value-count`** - Get count of high-value transactions (amount > 100,000)
3. **GET `/api/manager-transactions/export/csv`** - Export filtered transactions to CSV
4. **GET `/api/manager-transactions/export/excel`** - Export filtered transactions to Excel
5. **GET `/api/manager-transactions/statistics`** - Get transaction statistics summary
6. **GET `/api/manager-transactions/filter-options`** - Get available filter values

### Query Parameters:
- **Pagination**: `pageNumber`, `pageSize`
- **Search**: `searchText` (searches TransactionId or AccountId)
- **Filters**: `status`, `type`, `minAmount`, `maxAmount`, `startDate`, `endDate`, `viewMode`, `flag`

---

## 📦 Step 1: Update TypeScript Interface

Create or update `src/app/models/transaction.models.ts`:

```typescript
// Enums matching backend
export enum TransactionStatus {
  Completed = 0,
  Pending = 1,
  Rejected = 2,
  Failed = 3
}

export enum TransactionType {
  Deposit = 0,
  Withdrawal = 1,
  Transfer = 2
}

// Transaction interface
export interface Transaction {
  transactionId: number;
  accountId: number | null;
  type: TransactionType;
  amount: number;
  date: string | Date;
  status: TransactionStatus;
  flag: string;
}

// Paged result from backend
export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

// Transaction statistics
export interface TransactionStatistics {
  totalCount: number;
  highValueCount: number;
  completedCount: number;
  pendingCount: number;
  rejectedCount: number;
  failedCount: number;
  totalAmount: number;
  averageAmount: number;
  depositCount: number;
  withdrawalCount: number;
  transferCount: number;
}

// Filter options
export interface FilterOptions {
  statuses: string[];
  types: string[];
  flags: string[];
}
```

---

## 🔧 Step 2: Create Manager Transactions Service

Create `src/app/services/manager-transactions.service.ts`:

```typescript
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  Transaction, 
  PagedResult, 
  TransactionStatistics, 
  FilterOptions 
} from '../models/transaction.models';

@Injectable({
  providedIn: 'root'
})
export class ManagerTransactionsService {
  private apiUrl = 'https://localhost:7021/api/manager-transactions';

  constructor(private http: HttpClient) {}

  /**
   * Get paginated transactions with filters
   */
  getTransactions(params: {
    pageNumber?: number;
    pageSize?: number;
    searchText?: string;
    status?: string;
    type?: string;
    minAmount?: number;
    maxAmount?: number;
    startDate?: string;
    endDate?: string;
    viewMode?: 'all' | 'highvalue';
    flag?: string;
  }): Observable<PagedResult<Transaction>> {
    let httpParams = new HttpParams();

    // Add all parameters if they exist
    Object.keys(params).forEach(key => {
      const value = (params as any)[key];
      if (value !== null && value !== undefined && value !== '') {
        httpParams = httpParams.set(key, value.toString());
      }
    });

    return this.http.get<PagedResult<Transaction>>(this.apiUrl, { params: httpParams });
  }

  /**
   * Get count of high-value transactions
   */
  getHighValueCount(): Observable<{ highValueCount: number }> {
    return this.http.get<{ highValueCount: number }>(`${this.apiUrl}/high-value-count`);
  }

  /**
   * Get transaction statistics
   */
  getStatistics(): Observable<TransactionStatistics> {
    return this.http.get<TransactionStatistics>(`${this.apiUrl}/statistics`);
  }

  /**
   * Get available filter options
   */
  getFilterOptions(): Observable<FilterOptions> {
    return this.http.get<FilterOptions>(`${this.apiUrl}/filter-options`);
  }

  /**
   * Export to CSV
   */
  exportToCSV(filters: {
    searchText?: string;
    status?: string;
    type?: string;
    minAmount?: number;
    maxAmount?: number;
    startDate?: string;
    endDate?: string;
    viewMode?: 'all' | 'highvalue';
  }): void {
    let httpParams = new HttpParams();

    Object.keys(filters).forEach(key => {
      const value = (filters as any)[key];
      if (value !== null && value !== undefined && value !== '') {
        httpParams = httpParams.set(key, value.toString());
      }
    });

    const url = `${this.apiUrl}/export/csv?${httpParams.toString()}`;
    window.open(url, '_blank');
  }

  /**
   * Export to Excel
   */
  exportToExcel(filters: {
    searchText?: string;
    status?: string;
    type?: string;
    minAmount?: number;
    maxAmount?: number;
    startDate?: string;
    endDate?: string;
    viewMode?: 'all' | 'highvalue';
  }): void {
    let httpParams = new HttpParams();

    Object.keys(filters).forEach(key => {
      const value = (filters as any)[key];
      if (value !== null && value !== undefined && value !== '') {
        httpParams = httpParams.set(key, value.toString());
      }
    });

    const url = `${this.apiUrl}/export/excel?${httpParams.toString()}`;
    window.open(url, '_blank');
  }
}
```

---

## 🎨 Step 3: Update Transaction Table Component

Update `transaction-table.component.ts`:

```typescript
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ManagerTransactionsService } from '../../services/manager-transactions.service';
import { 
  Transaction, 
  TransactionStatus, 
  TransactionType,
  FilterOptions 
} from '../../models/transaction.models';

@Component({
  selector: 'app-transaction-table',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './transaction-table.component.html',
  styleUrl: './transaction-table.component.css'
})
export class TransactionTableComponent implements OnInit, OnDestroy {
  displayedColumns: string[] = ['id', 'accountId', 'type', 'amount', 'date', 'status', 'flag'];
  transactions: Transaction[] = [];
  pageSize = 5;
  currentPage = 1;  // Backend uses 1-based indexing
  totalPages = 0;
  totalCount = 0;
  Math = Math;

  // Filter properties
  searchText = '';
  selectedStatus = '';
  selectedType = '';
  minAmount = '';
  maxAmount = '';
  startDate: string = '';
  endDate: string = '';
  viewMode: 'all' | 'highvalue' = 'all';

  // Filter options from backend
  statuses: string[] = [];
  types: string[] = [];
  flags: string[] = [];

  // High-value count
  highValueCount = 0;

  // Loading state
  loading = false;

  private destroy$ = new Subject<void>();

  constructor(private transactionService: ManagerTransactionsService) {}

  ngOnInit() {
    // Load filter options
    this.loadFilterOptions();
    
    // Load initial transactions
    this.loadTransactions();
    
    // Load high-value count
    this.loadHighValueCount();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadFilterOptions() {
    this.transactionService.getFilterOptions()
      .pipe(takeUntil(this.destroy$))
      .subscribe(options => {
        this.statuses = options.statuses;
        this.types = options.types;
        this.flags = options.flags;
      });
  }

  loadTransactions() {
    this.loading = true;

    const params = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      searchText: this.searchText || undefined,
      status: this.selectedStatus || undefined,
      type: this.selectedType || undefined,
      minAmount: this.minAmount ? parseFloat(this.minAmount) : undefined,
      maxAmount: this.maxAmount ? parseFloat(this.maxAmount) : undefined,
      startDate: this.startDate || undefined,
      endDate: this.endDate || undefined,
      viewMode: this.viewMode
    };

    this.transactionService.getTransactions(params)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.transactions = result.items;
          this.totalCount = result.totalCount;
          this.totalPages = result.totalPages;
          this.loading = false;
        },
        error: (err) => {
          console.error('Error loading transactions:', err);
          this.loading = false;
        }
      });
  }

  loadHighValueCount() {
    this.transactionService.getHighValueCount()
      .pipe(takeUntil(this.destroy$))
      .subscribe(result => {
        this.highValueCount = result.highValueCount;
      });
  }

  applyFilters() {
    this.currentPage = 1;  // Reset to first page
    this.loadTransactions();
  }

  toggleHighValueFilter(mode: 'all' | 'highvalue'): void {
    this.viewMode = mode;
    this.applyFilters();
  }

  resetFilters() {
    this.searchText = '';
    this.selectedStatus = '';
    this.selectedType = '';
    this.minAmount = '';
    this.maxAmount = '';
    this.startDate = '';
    this.endDate = '';
    this.viewMode = 'all';
    this.applyFilters();
  }

  // Pagination methods
  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadTransactions();
    }
  }

  previousPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadTransactions();
    }
  }

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadTransactions();
    }
  }

  // Export methods
  exportToCSV() {
    const filters = {
      searchText: this.searchText || undefined,
      status: this.selectedStatus || undefined,
      type: this.selectedType || undefined,
      minAmount: this.minAmount ? parseFloat(this.minAmount) : undefined,
      maxAmount: this.maxAmount ? parseFloat(this.maxAmount) : undefined,
      startDate: this.startDate || undefined,
      endDate: this.endDate || undefined,
      viewMode: this.viewMode
    };

    this.transactionService.exportToCSV(filters);
  }

  exportToExcel() {
    const filters = {
      searchText: this.searchText || undefined,
      status: this.selectedStatus || undefined,
      type: this.selectedType || undefined,
      minAmount: this.minAmount ? parseFloat(this.minAmount) : undefined,
      maxAmount: this.maxAmount ? parseFloat(this.maxAmount) : undefined,
      startDate: this.startDate || undefined,
      endDate: this.endDate || undefined,
      viewMode: this.viewMode
    };

    this.transactionService.exportToExcel(filters);
  }

  // Helper methods for display
  getStatusClass(status: number): string {
    switch (status) {
      case TransactionStatus.Completed: return 'status-completed';
      case TransactionStatus.Pending: return 'status-pending';
      case TransactionStatus.Rejected: return 'status-rejected';
      case TransactionStatus.Failed: return 'status-failed';
      default: return '';
    }
  }

  getStatusText(status: number): string {
    return TransactionStatus[status];
  }

  getTypeText(type: number): string {
    return TransactionType[type];
  }

  formatDate(date: string | Date): string {
    return new Date(date).toLocaleDateString();
  }

  formatAmount(amount: number): string {
    return '₹' + amount.toLocaleString();
  }

  getHighValueBadge(): string {
    return this.viewMode === 'highvalue' ? 'Showing High-Value Only' : 'Showing All Transactions';
  }
}
```

---

## 🖼️ Step 4: Update HTML Template (Key Changes)

Update `transaction-table.component.html` to use backend pagination:

```html
<!-- Filter Section -->
<div class="filter-section">
  <!-- Search -->
  <input
    type="text"
    [(ngModel)]="searchText"
    placeholder="Search by Transaction ID or Account ID"
    (input)="applyFilters()"
  />

  <!-- Status Filter -->
  <select [(ngModel)]="selectedStatus" (change)="applyFilters()">
    <option value="">All Statuses</option>
    <option *ngFor="let status of statuses" [value]="status">{{ status }}</option>
  </select>

  <!-- Type Filter -->
  <select [(ngModel)]="selectedType" (change)="applyFilters()">
    <option value="">All Types</option>
    <option *ngFor="let type of types" [value]="type">{{ type }}</option>
  </select>

  <!-- Amount Range -->
  <input
    type="number"
    [(ngModel)]="minAmount"
    placeholder="Min Amount"
    (change)="applyFilters()"
  />
  <input
    type="number"
    [(ngModel)]="maxAmount"
    placeholder="Max Amount"
    (change)="applyFilters()"
  />

  <!-- Date Range -->
  <input
    type="date"
    [(ngModel)]="startDate"
    (change)="applyFilters()"
  />
  <input
    type="date"
    [(ngModel)]="endDate"
    (change)="applyFilters()"
  />

  <!-- View Mode Toggle -->
  <button
    [class.active]="viewMode === 'all'"
    (click)="toggleHighValueFilter('all')"
  >
    All ({{ totalCount }})
  </button>
  <button
    [class.active]="viewMode === 'highvalue'"
    (click)="toggleHighValueFilter('highvalue')"
  >
    High-Value ({{ highValueCount }})
  </button>

  <!-- Reset & Export -->
  <button (click)="resetFilters()">Reset Filters</button>
  <button (click)="exportToCSV()">Export CSV</button>
  <button (click)="exportToExcel()">Export Excel</button>
</div>

<!-- Loading Indicator -->
<div *ngIf="loading" class="loading">Loading transactions...</div>

<!-- Table -->
<table *ngIf="!loading">
  <thead>
    <tr>
      <th>Transaction ID</th>
      <th>Account ID</th>
      <th>Type</th>
      <th>Amount</th>
      <th>Date</th>
      <th>Status</th>
      <th>Flag</th>
    </tr>
  </thead>
  <tbody>
    <tr *ngFor="let txn of transactions">
      <td>{{ txn.transactionId }}</td>
      <td>{{ txn.accountId || 'N/A' }}</td>
      <td>{{ getTypeText(txn.type) }}</td>
      <td>{{ formatAmount(txn.amount) }}</td>
      <td>{{ formatDate(txn.date) }}</td>
      <td>
        <span [class]="getStatusClass(txn.status)">
          {{ getStatusText(txn.status) }}
        </span>
      </td>
      <td>{{ txn.flag }}</td>
    </tr>
  </tbody>
</table>

<!-- Pagination (Backend) -->
<div class="pagination">
  <button (click)="previousPage()" [disabled]="currentPage === 1">Previous</button>
  <span>Page {{ currentPage }} of {{ totalPages }} (Total: {{ totalCount }})</span>
  <button (click)="nextPage()" [disabled]="currentPage === totalPages">Next</button>
</div>
```

---

## ✅ Step 5: Verify Integration

### Backend:
1. Run the backend: `dotnet run`
2. Test endpoints with `ManagerTransactionsApi.http` file

### Frontend:
1. Ensure JWT token is included in HTTP requests (use HTTP interceptor)
2. Test filtering, pagination, and export functionality
3. Check browser Network tab to verify API calls

---

## 🎯 Key Differences from Old Code

| Old (Client-Side) | New (Server-Side) |
|---|---|
| `getTransactions()` returns all data | `getTransactions()` returns paginated data |
| Filters applied in Angular | Filters applied in backend |
| Pagination on filtered array | Pagination handled by backend |
| Export using local data | Export via backend endpoints |
| High-value filter in TypeScript | High-value filter in SQL query |

---

## 📚 Benefits of Backend Integration

✅ **Performance**: Backend filters millions of records efficiently  
✅ **Scalability**: Works with large datasets  
✅ **Security**: Filters applied server-side (no data exposure)  
✅ **Consistency**: Single source of truth for transaction data  
✅ **Export**: CSV/Excel generated with current filters applied

---

## 🔧 Troubleshooting

### Issue: 401 Unauthorized
**Solution**: Ensure JWT token is valid and includes "Manager" role claim

### Issue: Empty results
**Solution**: Check filter parameters match backend enum values (case-sensitive)

### Issue: Export doesn't work
**Solution**: Ensure JWT token is included in download URL (may need custom download logic)

---

## 📞 Need Help?

- **Backend API Documentation**: See `ManagerTransactionsApi.http`
- **Test Endpoints**: Use REST client (VS Code extension) or Postman
- **Debug**: Check browser DevTools Network tab and backend console logs

---

**Last Updated:** February 25, 2026  
**Author:** Development Team  
**Version:** 1.0
