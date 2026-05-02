"use client";

import { useEffect, useState } from "react";
import { SetupVAT, CreateSetupVATRequest, UpdateSetupVATRequest, SetupCountry } from "../../../../types/settings";
import { vatService } from "@/services/vat-service";
import { countryService } from "@/services/country-service";
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
    DialogTrigger,
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

export function VATSettings() {
    const { selectedCompany, selectedLocation } = useAuth();
    const [vats, setVats] = useState<SetupVAT[]>([]);
    const [countries, setCountries] = useState<SetupCountry[]>([]);
    const [loading, setLoading] = useState(true);
    const [isAddModalOpen, setIsAddModalOpen] = useState(false);
    const [editingVat, setEditingVat] = useState<SetupVAT | null>(null);

    // Form State
    const [formData, setFormData] = useState<Partial<CreateSetupVATRequest>>({
        name: "",
        rate: 0,
        description: "",
        countryCode: "",
        isActive: true,
    });

    useEffect(() => {
        loadVats();
    }, [selectedCompany?.companyId]);

    const loadVats = async () => {
        if (!selectedCompany?.companyId) return;
        setLoading(true);
        try {
            const [vatsData, countriesData] = await Promise.all([
                vatService.getVATs(),
                countryService.getCountries()
            ]);
            setVats(vatsData);
            setCountries(countriesData.filter(c => c.isActive));
        } catch (error) {
            toast.error("Failed to load VAT configurations.");
        } finally {
            setLoading(false);
        }
    };

    const handleOpenAdd = () => {
        setFormData({
            name: "",
            rate: 0,
            description: "",
            countryCode: "",
            isActive: true,
            companyId: selectedCompany?.companyId,
            locationId: selectedLocation?.id,
        });
        setEditingVat(null);
        setIsAddModalOpen(true);
    };

    const handleOpenEdit = (vat: SetupVAT) => {
        setFormData({
            name: vat.name,
            rate: vat.rate,
            description: vat.description || "",
            countryCode: vat.countryCode || "",
            isActive: vat.isActive,
            companyId: vat.companyId,
            locationId: vat.locationId,
        });
        setEditingVat(vat);
        setIsAddModalOpen(true);
    };

    const handleSubmit = async () => {
        if (!formData.name) {
            toast.error("Name is required");
            return;
        }

        try {
            if (editingVat) {
                await vatService.updateVAT(editingVat.id, formData as UpdateSetupVATRequest);
                toast.success("VAT updated successfully");
            } else {
                await vatService.createVAT(formData as CreateSetupVATRequest);
                toast.success("VAT created successfully");
            }
            setIsAddModalOpen(false);
            loadVats();
        } catch (error: any) {
            toast.error(error.response?.data?.message || "Failed to save VAT");
        }
    };

    const handleDelete = async (id: string) => {
        try {
            await vatService.deleteVAT(id);
            toast.success("VAT deleted successfully");
            loadVats();
        } catch (error: any) {
            toast.error(error.response?.data?.message || "Failed to delete VAT");
        }
    };

    return (
        <Card>
            <CardHeader className="flex flex-row items-center justify-between">
                <div>
                    <CardTitle>VAT Configurations</CardTitle>
                    <CardDescription>
                        Manage Value Added Tax rates and settings.
                    </CardDescription>
                </div>
                <Button onClick={handleOpenAdd}>
                    <Plus className="mr-2 h-4 w-4" /> Add VAT
                </Button>
            </CardHeader>
            <CardContent>
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead>Name</TableHead>
                            <TableHead>Rate (%)</TableHead>
                            <TableHead>Country Code</TableHead>
                            <TableHead>Description</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {loading ? (
                            <TableRow>
                                <TableCell colSpan={6} className="text-center py-8 text-muted-foreground">
                                    Loading VAT configurations...
                                </TableCell>
                            </TableRow>
                        ) : vats.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={6} className="text-center py-8 text-muted-foreground">
                                    No VAT configurations found. Click "Add VAT" to create one.
                                </TableCell>
                            </TableRow>
                        ) : (
                            vats.map((vat) => (
                                <TableRow key={vat.id}>
                                    <TableCell className="font-medium">{vat.name}</TableCell>
                                    <TableCell>{vat.rate}%</TableCell>
                                    <TableCell>{vat.countryCode || "-"}</TableCell>
                                    <TableCell className="max-w-xs truncate">{vat.description || "-"}</TableCell>
                                    <TableCell>
                                        <Badge variant={vat.isActive ? "default" : "secondary"}>
                                            {vat.isActive ? "Active" : "Inactive"}
                                        </Badge>
                                    </TableCell>
                                    <TableCell className="text-right space-x-2">
                                        <Button variant="outline" size="icon" onClick={() => handleOpenEdit(vat)}>
                                            <Pencil className="h-4 w-4" />
                                        </Button>
                                        <AlertDialog>
                                            <AlertDialogTrigger asChild>
                                                <Button variant="destructive" size="icon">
                                                    <Trash2 className="h-4 w-4" />
                                                </Button>
                                            </AlertDialogTrigger>
                                            <AlertDialogContent>
                                                <AlertDialogHeader>
                                                    <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
                                                    <AlertDialogDescription>
                                                        This action cannot be undone. This will permanently delete the VAT configuration '{vat.name}'.
                                                    </AlertDialogDescription>
                                                </AlertDialogHeader>
                                                <AlertDialogFooter>
                                                    <AlertDialogCancel>Cancel</AlertDialogCancel>
                                                    <AlertDialogAction onClick={() => handleDelete(vat.id)} className="bg-red-600 hover:bg-red-700">
                                                        Delete
                                                    </AlertDialogAction>
                                                </AlertDialogFooter>
                                            </AlertDialogContent>
                                        </AlertDialog>
                                    </TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>
            </CardContent>

            <Dialog open={isAddModalOpen} onOpenChange={setIsAddModalOpen}>
                <DialogContent>
                    <DialogHeader>
                        <DialogTitle>{editingVat ? "Edit VAT Configuration" : "Add VAT Configuration"}</DialogTitle>
                        <DialogDescription>
                            Enter the details for this tax rate.
                        </DialogDescription>
                    </DialogHeader>
                    <div className="grid gap-4 py-4">
                        <div className="grid grid-cols-4 items-center gap-4">
                            <Label htmlFor="name" className="text-right">Name</Label>
                            <Input
                                id="name"
                                value={formData.name}
                                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                                className="col-span-3"
                                placeholder="e.g. Standard VAT"
                            />
                        </div>
                        <div className="grid grid-cols-4 items-center gap-4">
                            <Label htmlFor="rate" className="text-right">Rate (%)</Label>
                            <Input
                                id="rate"
                                type="number"
                                step="0.01"
                                value={formData.rate}
                                onChange={(e) => setFormData({ ...formData, rate: parseFloat(e.target.value) || 0 })}
                                className="col-span-3"
                            />
                        </div>
                        <div className="grid grid-cols-4 items-center gap-4">
                            <Label htmlFor="countryCode" className="text-right">Country</Label>
                            <div className="col-span-3">
                                <Select
                                    value={formData.countryCode || ""}
                                    onValueChange={(val) => setFormData({ ...formData, countryCode: val })}
                                >
                                    <SelectTrigger>
                                        <SelectValue placeholder="Select a configured country" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        {countries.map(c => (
                                            <SelectItem key={c.id} value={c.countryCode}>
                                                {c.countryName} ({c.countryCode})
                                            </SelectItem>
                                        ))}
                                        {countries.length === 0 && (
                                            <div className="p-2 text-sm text-muted-foreground text-center">
                                                No countries configured.
                                            </div>
                                        )}
                                    </SelectContent>
                                </Select>
                            </div>
                        </div>
                        <div className="grid grid-cols-4 items-start gap-4">
                            <Label htmlFor="description" className="text-right mt-2">Description</Label>
                            <Textarea
                                id="description"
                                value={formData.description}
                                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                                className="col-span-3"
                            />
                        </div>
                        <div className="grid grid-cols-4 items-center gap-4">
                            <Label htmlFor="isActive" className="text-right">Active</Label>
                            <div className="col-span-3 flex items-center space-x-2">
                                <Switch
                                    id="isActive"
                                    checked={formData.isActive}
                                    onCheckedChange={(checked: boolean) => setFormData({ ...formData, isActive: checked })}
                                />
                                <Label htmlFor="isActive">{formData.isActive ? "Yes" : "No"}</Label>
                            </div>
                        </div>
                    </div>
                    <DialogFooter>
                        <Button variant="outline" onClick={() => setIsAddModalOpen(false)}>Cancel</Button>
                        <Button onClick={handleSubmit}>Save</Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </Card>
    );
}
