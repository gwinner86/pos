import { ExpensesClient } from "./client";

export default function ExpensesPage() {
    return (
        <div className="flex-1 space-y-4 p-8 pt-6">
            <ExpensesClient />
        </div>
    );
}
