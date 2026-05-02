import {
    LayoutDashboard,
    ShoppingCart,
    Package,
    Users,
    Settings,
    FileText,
    Truck,
    Wallet,
    Layers,
    ClipboardList
} from "lucide-react";
import { LucideIcon } from "lucide-react";

export interface NavItem {
    title: string;
    href: string;
    icon: LucideIcon;
    matches?: string[];
    children?: NavItem[];
    requirePermission?: string;
}

export const navItems: NavItem[] = [
    { title: "Dashboard", href: "/dashboard", icon: LayoutDashboard, requirePermission: "Menu.Dashboard" },
    { title: "Point of Sale", href: "/dashboard/pos", icon: ShoppingCart, requirePermission: "Menu.POS" },
    { title: "Sales History", href: "/dashboard/sales", icon: FileText, requirePermission: "Menu.SalesHistory" },
    {
        title: "Inventory",
        href: "/dashboard/inventory",
        icon: Package,
        requirePermission: "Menu.Inventory.View",
        children: [
            { title: "Products", href: "/dashboard/inventory/products", icon: Package, requirePermission: "Menu.Inventory.Products" },
            { title: "Categories", href: "/dashboard/inventory/categories", icon: Layers, requirePermission: "Menu.Inventory.Categories" },
            { title: "Adjustments", href: "/dashboard/inventory/adjustments", icon: ClipboardList, requirePermission: "Menu.Inventory.Adjustments" }
        ]
    },
    {
        title: "Procurement",
        href: "/dashboard/procurement",
        icon: Truck,
        requirePermission: "Menu.Procurement.View",
        children: [
            { title: "Suppliers", href: "/dashboard/procurement/suppliers", icon: Users, requirePermission: "Menu.Procurement.Suppliers" },
            { title: "Instant Purchases", href: "/dashboard/procurement/instant-purchases", icon: ShoppingCart, requirePermission: "Menu.Procurement.InstantPurchases" },
            { title: "Purchase Orders", href: "/dashboard/procurement/purchase-orders", icon: ClipboardList, requirePermission: "Menu.Procurement.PurchaseOrders" },
            { title: "Goods Receipts", href: "/dashboard/procurement/goods-receipts", icon: Package, requirePermission: "Menu.Procurement.GoodsReceipts" },
            { title: "Vendor Invoices", href: "/dashboard/procurement/vendor-invoices", icon: FileText, requirePermission: "Menu.Procurement.VendorInvoices" },
            { title: "Vendor Payments", href: "/dashboard/procurement/vendor-payments", icon: Wallet, requirePermission: "Menu.Procurement.VendorPayments" }
        ]
    },
    { title: "Customers", href: "/dashboard/customers", icon: Users, requirePermission: "Menu.Customers" },
    {
        title: "Accounting",
        href: "/dashboard/accounting",
        icon: Wallet,
        requirePermission: "Menu.Accounting.View",
        children: [
            { title: "Financials", href: "/dashboard/accounting", icon: FileText, requirePermission: "Menu.Accounting.Financials" },
            { title: "Transactions", href: "/dashboard/accounting/transactions", icon: ClipboardList, requirePermission: "Menu.Accounting.Transactions" },
            { title: "Expenses", href: "/dashboard/accounting/expenses", icon: Wallet, requirePermission: "Menu.Accounting.Expenses" },
            { title: "Income Statement", href: "/dashboard/accounting/reports/income-statement", icon: FileText, requirePermission: "Menu.Accounting.IncomeStatement" },
            { title: "Balance Sheet", href: "/dashboard/accounting/reports/balance-sheet", icon: FileText, requirePermission: "Menu.Accounting.BalanceSheet" },
            { title: "Trial Balance", href: "/dashboard/accounting/reports/trial-balance", icon: FileText, requirePermission: "Menu.Accounting.TrialBalance" }
        ]
    },
    {
        title: "Reports",
        href: "/dashboard/reports",
        icon: FileText,
        requirePermission: "Menu.Reports.View",
        children: [
            { title: "User Transactions", href: "/dashboard/reports/user-sales", icon: Users, requirePermission: "Menu.Reports.UserSales" }
        ]
    },
    { title: "Settings", href: "/dashboard/settings", icon: Settings, requirePermission: "Menu.Settings" },
];

export function getFilteredNavItems(items: NavItem[], userPermissions: string[] | undefined): NavItem[] {
    if (!userPermissions) return items; // Fallback or could return []

    const filtered: NavItem[] = [];

    for (const item of items) {
        // If the item itself is restricted and user lacks permission, skip it entirely
        if (item.requirePermission && !userPermissions.includes(item.requirePermission)) {
            continue;
        }

        // If it has children, recursively filter them
        if (item.children) {
            const filteredChildren = getFilteredNavItems(item.children, userPermissions);
            // If all children were filtered out and it only acts as a container, you might choose to skip the parent too.
            // But since parent has its own `requirePermission`, we keep it if parent is allowed.
            filtered.push({ ...item, children: filteredChildren });
        } else {
            filtered.push(item);
        }
    }

    return filtered;
}
