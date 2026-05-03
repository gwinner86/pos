"use client";

import { useEffect, useState } from "react";
import { SetupExpenseType, CreateSetupExpenseTypeRequest, UpdateSetupExpenseTypeRequest } from "../../../../types/settings";
import { expenseTypeService } from "@/services/expense-type-service";
import { accountingService } from "@/services/accounting-service";
import { GLAccount } from "@/types/accounting";
import { useAuth } from "@/hooks/use-auth";
import { toast } from "sonner";
import { format } from "date-fns";

import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import {
    AlertDialog,
    AlertDialogAction,
    AlertDialogCancel,
    AlertDialogContent,
    AlertDialogDescription,
    AlertDialogFooter,
    AlertDialogHeader,
    AlertDialogTitle,
    AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Badge } from "@/components/ui/badge";
import { Plus, Pencil, Trash2 } from "lucide-react";
import { Textarea } from "@/components/ui/textarea";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";

export function ExpenseTypeSettings() {
    const { selectedCompany } = useAuth();
    const [expenseTypes, setExpenseTypes] = useState<SetupExpenseType[]>([]);
    const [accounts, setAccounts] = useState<GLAccount[]>([]);
    const [loading, setLoading] = useState(true);
    const [isAddModalOpen, setIsAddModalOpen] = useState(false);
    const [editingType, setEditingType] = useState<SetupExpenseType | null>(null);

    // Form State
    const [formData, setFormData] = useState<Partial<CreateSetupExpenseTypeRequest>>({
        name: "",
        description: "",
        expenseAccountId: "",
        isActive: true,
    });

    useEffect(() => {
        loadData();
    }, [selectedCompany?.companyId]);

    const loadData = async () => {
        if (!selectedCompany?.companyId) return;
        setLoading(true);
        try {
            const [typesData, accountsData] = await Promise.all([
                expenseTypeService.getExpenseTypes(),
                accountingService.getGLAccounts()
            ]);
            setExpenseTypes(typesData);
            setAccounts(accountsData);
        } catch (error) {
            toast.error("Failed to load Expense Types.");
        } finally {
            setLoading(false);
        }
    };

    const handleOpenAdd = () => {
        setFormData({
            name: "",
            description: "",
            expenseAccountId: "",
            isActive: true,
        });
        setEditingType(null);
        setIsAddModalOpen(true);
    };

    const handleOpenEdit = (type: SetupExpenseType) => {
        setFormData({
            name: type.name,
            description: type.description || "",
            expenseAccountId: type.expenseAccountId,
            isActive: type.isActive,
        });
        setEditingType(type);
        setIsAddModalOpen(true);
    };

    const handleSubmit = async () => {
        if (!formData.name || !formData.expenseAccountId) {
            toast.error("Name and GL Account are required");
            return;
        }

        try {
            if (editingType) {
                await expenseTypeService.updateExpenseType(editingType.id, formData as UpdateSetupExpenseTypeRequest);
                toast.success("Expense Type updated successfully");
            } else {
                await expenseTypeService.createExpenseType(formData as CreateSetupExpenseTypeRequest);
                toast.success("Expense Type created successfully");
            }
            setIsAddModalOpen(false);
            loadData();
        } catch (error: any) {
            toast.error(error.response?.data?.message || "Failed to save Expense Type");
        }
    };

    const handleDelete = async (id: string) => {
        try {
            await expenseTypeService.deleteExpenseType(id);
            toast.success("Expense Type deleted successfully");
            loadData();
        } catch (error) {
            toast.error("Failed to delete Expense Type");
        }
    };

    return (
        <Card>
            <CardHeader className="flex flex-row items-center justify-between">
                <div>
                    <CardTitle>Expense Types</CardTitle>
                    <CardDescription>
                        Manage operational expense categories and map them to General Ledger (GL) accounts.
                    </CardDescription>
                </div>
                <Button onClick={handleOpenAdd}>
                    <Plus className="h-4 w-4 mr-2" /> Add Expense Type
                </Button>
            </CardHeader>
            <CardContent>
                <div className="rounded-md border">
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Name</TableHead>
                                <TableHead>Description</TableHead>
                                <TableHead>Mapped GL Account</TableHead>
                                <TableHead>Status</TableHead>
                                <TableHead className="text-right">Actions</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {loading ? (
                                <TableRow>
                                    <TableCell colSpan={5} className="text-center py-8 text-muted-foreground">
                                        Loading expense types...
                                    </TableCell>
                                </TableRow>
                            ) : expenseTypes.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={5} className="text-center py-8 text-muted-foreground">
                                        No expense types configured.
                                    </TableCell>
                                </TableRow>
                            ) : (
                                expenseTypes.map((type) => (
                                    <TableRow key={type.id}>
                                        <TableCell className="font-medium">{type.name}</TableCell>
                                        <TableCell>{type.description || "-"}</TableCell>
                                        <TableCell>{type.expenseAccountName}</TableCell>
                                        <TableCell>
                                            <Badge variant={type.isActive ? "default" : "secondary"}>
                                                {type.isActive ? "Active" : "Inactive"}
                                            </Badge>
                                        </TableCell>
                                        <TableCell className="text-right">
                                            <div className="flex justify-end gap-2">
                                                <Button
                                                    variant="ghost"
                                                    size="icon"
                                                    onClick={() => handleOpenEdit(type)}
                                                >
                                                    <Pencil className="h-4 w-4" />
                                                </Button>

                                                <AlertDialog>
                                                    <AlertDialogTrigger asChild>
                                                        <Button variant="ghost" size="icon" className="text-red-500 hover:text-red-600">
                                                            <Trash2 className="h-4 w-4" />
                                                        </Button>
                                                    </AlertDialogTrigger>
                                                    <AlertDialogContent>
                                                        <AlertDialogHeader>
                                                            <AlertDialogTitle>Delete Expense Type</AlertDialogTitle>
                                                            <AlertDialogDescription>
                                                                Are you sure you want to delete {type.name}? This action cannot be undone.
                                                            </AlertDialogDescription>
                                                        </AlertDialogHeader>
                                                        <AlertDialogFooter>
                                                            <AlertDialogCancel>Cancel</AlertDialogCancel>
                                                            <AlertDialogAction
                                                                onClick={() => handleDelete(type.id)}
                                                                className="bg-red-500 hover:bg-red-600"
                                                            >
                                                                Delete
                                                            </AlertDialogAction>
                                                        </AlertDialogFooter>
                                                    </AlertDialogContent>
                                                </AlertDialog>
                                            </div>
                                        </TableCell>
                                    </TableRow>
                                ))
                            )}
                        </TableBody>
                    </Table>
                </div>

                <Dialog open={isAddModalOpen} onOpenChange={setIsAddModalOpen}>
                    <DialogContent>
                        <DialogHeader>
                            <DialogTitle>{editingType ? "Edit Expense Type" : "Add Expense Type"}</DialogTitle>
                            <DialogDescription>
                                Document a new expense type and link it to the chart of accounts.
                            </DialogDescription>
                        </DialogHeader>

                        <div className="grid gap-4 py-4">
                            <div className="grid gap-2">
                                <Label htmlFor="name">Name</Label>
                                <Input
                                    id="name"
                                    value={formData.name}
                                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                                />
                            </div>

                            <div className="grid gap-2">
                                <Label htmlFor="description">Description (Optional)</Label>
                                <Textarea
                                    id="description"
                                    value={formData.description}
                                    onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                                />
                            </div>

                            <div className="grid gap-2">
                                <Label>GL Account Mapping</Label>
                                <Select
                                    value={formData.expenseAccountId}
                                    onValueChange={(val) => setFormData({ ...formData, expenseAccountId: val })}
                                >
                                    <SelectTrigger>
                                        <SelectValue placeholder="Select GL Account" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        {accounts.filter(a => a.accountType === "Expense").map((account) => (
                                            <SelectItem key={account.id} value={account.id}>
                                                {account.accountNumber} - {account.accountName}
                                            </SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                            </div>

                            <div className="flex items-center space-x-2 mt-2">
                                <Switch
                                    id="isActive"
                                    checked={formData.isActive}
                                    onCheckedChange={(checked) => setFormData({ ...formData, isActive: checked })}
                                />
                                <Label htmlFor="isActive">Active Status</Label>
                            </div>
                        </div>

                        <DialogFooter>
                            <Button variant="outline" onClick={() => setIsAddModalOpen(false)}>
                                Cancel
                            </Button>
                            <Button onClick={handleSubmit}>
                                Save Expense Type
                            </Button>
                        </DialogFooter>
                    </DialogContent>
                </Dialog>
            </CardContent>
        </Card>
    );
}
