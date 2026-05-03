"use client";

import { useEffect, useState } from "react";
import { SetupCountry, CreateSetupCountryRequest, UpdateSetupCountryRequest } from "@/types/settings";
import { countryService } from "@/services/country-service";
import { useAuth } from "@/hooks/use-auth";
import { toast } from "sonner";

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

export function CountrySettings() {
    const { selectedCompany, selectedLocation } = useAuth();
    const [countries, setCountries] = useState<SetupCountry[]>([]);
    const [loading, setLoading] = useState(true);
    const [isAddModalOpen, setIsAddModalOpen] = useState(false);
    const [editingCountry, setEditingCountry] = useState<SetupCountry | null>(null);

    // Form State
    const [formData, setFormData] = useState<Partial<CreateSetupCountryRequest>>({
        countryName: "",
        countryCode: "",
    });
    const [isActive, setIsActive] = useState(true);

    useEffect(() => {
        loadCountries();
    }, [selectedCompany?.companyId]);

    const loadCountries = async () => {
        if (!selectedCompany?.companyId) return;
        setLoading(true);
        try {
            const data = await countryService.getCountries();
            setCountries(data);
        } catch (error) {
            toast.error("Failed to load configured countries.");
        } finally {
            setLoading(false);
        }
    };

    const handleOpenAdd = () => {
        setFormData({
            countryName: "",
            countryCode: "",
            companyId: selectedCompany?.companyId,
            locationId: selectedLocation?.id,
        });
        setIsActive(true);
        setEditingCountry(null);
        setIsAddModalOpen(true);
    };

    const handleOpenEdit = (country: SetupCountry) => {
        setFormData({
            countryName: country.countryName,
            countryCode: country.countryCode,
            companyId: country.companyId,
            locationId: country.locationId,
        });
        setIsActive(country.isActive);
        setEditingCountry(country);
        setIsAddModalOpen(true);
    };

    const handleSubmit = async () => {
        if (!formData.countryName || !formData.countryCode) {
            toast.error("Country Name and Type Code are required");
            return;
        }

        try {
            if (editingCountry) {
                const updateData: UpdateSetupCountryRequest = {
                    countryName: formData.countryName!,
                    countryCode: formData.countryCode!,
                    isActive: isActive
                };
                await countryService.updateCountry(editingCountry.id, updateData);
                toast.success("Country updated successfully");
            } else {
                const createData: CreateSetupCountryRequest = {
                    ...formData as CreateSetupCountryRequest,
                    tenantId: "00000000-0000-0000-0000-000000000000",
                    companyId: formData.companyId || "00000000-0000-0000-0000-000000000000",
                };
                await countryService.createCountry(createData);
                toast.success("Country added successfully");
            }
            setIsAddModalOpen(false);
            loadCountries();
        } catch (error: any) {
            toast.error(error.response?.data?.message || "Failed to save country");
        }
    };

    const handleDelete = async (id: string) => {
        try {
            await countryService.deleteCountry(id);
            toast.success("Country deleted successfully");
            loadCountries();
        } catch (error: any) {
            toast.error(error.response?.data?.message || "Failed to delete country");
        }
    };

    return (
        <Card>
            <CardHeader className="flex flex-row items-center justify-between">
                <div>
                    <CardTitle>Country Configurations</CardTitle>
                    <CardDescription>
                        Manage operational countries for determining tax zones.
                    </CardDescription>
                </div>
                <Button onClick={handleOpenAdd}>
                    <Plus className="mr-2 h-4 w-4" /> Add Country
                </Button>
            </CardHeader>
            <CardContent>
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead>Country Name</TableHead>
                            <TableHead>Country Code</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {loading ? (
                            <TableRow>
                                <TableCell colSpan={4} className="text-center py-8 text-muted-foreground">
                                    Loading configured countries...
                                </TableCell>
                            </TableRow>
                        ) : countries.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={4} className="text-center py-8 text-muted-foreground">
                                    No countries configured. Click "Add Country" to create one.
                                </TableCell>
                            </TableRow>
                        ) : (
                            countries.map((country) => (
                                <TableRow key={country.id}>
                                    <TableCell className="font-medium">{country.countryName}</TableCell>
                                    <TableCell>{country.countryCode}</TableCell>
                                    <TableCell>
                                        <Badge variant={country.isActive ? "default" : "secondary"}>
                                            {country.isActive ? "Active" : "Inactive"}
                                        </Badge>
                                    </TableCell>
                                    <TableCell className="text-right space-x-2">
                                        <Button variant="outline" size="icon" onClick={() => handleOpenEdit(country)}>
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
                                                        This action cannot be undone. This will permanently remove {country.countryName} setup.
                                                    </AlertDialogDescription>
                                                </AlertDialogHeader>
                                                <AlertDialogFooter>
                                                    <AlertDialogCancel>Cancel</AlertDialogCancel>
                                                    <AlertDialogAction onClick={() => handleDelete(country.id)} className="bg-red-600 hover:bg-red-700">
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
                        <DialogTitle>{editingCountry ? "Edit Country Configuration" : "Add Country Configuration"}</DialogTitle>
                        <DialogDescription>
                            Enter the country details below.
                        </DialogDescription>
                    </DialogHeader>
                    <div className="grid gap-4 py-4">
                        <div className="grid grid-cols-4 items-center gap-4">
                            <Label htmlFor="countryName" className="text-right">Country Name</Label>
                            <Input
                                id="countryName"
                                value={formData.countryName}
                                onChange={(e) => setFormData({ ...formData, countryName: e.target.value })}
                                className="col-span-3"
                                placeholder="e.g. Ghana"
                            />
                        </div>
                        <div className="grid grid-cols-4 items-center gap-4">
                            <Label htmlFor="countryCode" className="text-right">Country Code</Label>
                            <Input
                                id="countryCode"
                                value={formData.countryCode}
                                onChange={(e) => setFormData({ ...formData, countryCode: e.target.value.toUpperCase() })}
                                className="col-span-3"
                                placeholder="e.g. GH"
                                maxLength={5}
                            />
                        </div>

                        {editingCountry && (
                            <div className="grid grid-cols-4 items-center gap-4">
                                <Label htmlFor="isActive" className="text-right">Active</Label>
                                <div className="col-span-3 flex items-center space-x-2">
                                    <Switch
                                        id="isActive"
                                        checked={isActive}
                                        onCheckedChange={setIsActive}
                                    />
                                    <Label htmlFor="isActive">{isActive ? "Yes" : "No"}</Label>
                                </div>
                            </div>
                        )}
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
