"use client"

import { useEffect, useState } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import * as z from "zod"
import { Loader2, Plus, Pencil, MapPin } from "lucide-react"
import { toast } from "sonner"
import { settingsService, Location, Currency } from "@/services/settings-service"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card"
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table"
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
    DialogFooter,
    DialogDescription,
} from "@/components/ui/dialog"
import {
    Form,
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form"
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select"

const locationSchema = z.object({
    locationName: z.string().min(2, "Name is required"),
    locationType: z.string().min(2, "Type is required"), // e.g., 'Store' or 'Warehouse'
    addressLine1: z.string().optional(),
    currencyId: z.string().optional(), // For selection
    vatCalculationType: z.number().default(1),
})

type LocationFormValues = z.infer<typeof locationSchema>;

export function LocationSettings() {
    const [locations, setLocations] = useState<Location[]>([])
    const [currencies, setCurrencies] = useState<Currency[]>([])
    const [loading, setLoading] = useState(true)
    const [open, setOpen] = useState(false)
    const [editingLocation, setEditingLocation] = useState<Location | null>(null)

    const form = useForm<LocationFormValues>({
        resolver: zodResolver(locationSchema) as any,
        defaultValues: {
            locationName: "",
            locationType: "Store",
            addressLine1: "",
            currencyId: "",
            vatCalculationType: 1,
        },
    })

    useEffect(() => {
        loadData()
    }, [])

    useEffect(() => {
        if (!open) {
            setEditingLocation(null)
            form.reset({
                locationName: "",
                locationType: "Store",
                addressLine1: "",
                currencyId: "",
                vatCalculationType: 1,
            })
        }
    }, [open, form])

    const loadData = async () => {
        setLoading(true)
        try {
            const [locs, currs] = await Promise.all([
                settingsService.getLocations(),
                settingsService.getCurrencies()
            ])
            setLocations(locs || [])
            setCurrencies(currs || [])
        } catch (error) {
            console.error("Failed to load settings data", error)
            toast.error("Failed to load locations or currencies")
        } finally {
            setLoading(false)
        }
    }

    const onSubmit = async (data: z.infer<typeof locationSchema>) => {
        try {
            // Convert string currencyId to undefined if empty, or keep as is.
            // Backend expects Guid?
            const payload = {
                ...data,
                currencyId: data.currencyId === "none" || !data.currencyId ? undefined : data.currencyId
            }

            if (editingLocation) {
                await settingsService.updateLocation(editingLocation.id, payload)
                toast.success("Location updated successfully")
            } else {
                await settingsService.createLocation(payload)
                toast.success("Location created successfully")
            }
            setOpen(false)
            loadData()
        } catch (error) {
            toast.error(editingLocation ? "Failed to update location" : "Failed to create location")
        }
    }

    const handleEdit = (location: Location) => {
        setEditingLocation(location)
        form.reset({
            locationName: location.locationName,
            locationType: location.locationType,
            addressLine1: location.addressLine1 || "",
            currencyId: location.currencyId || "",
            vatCalculationType: location.vatCalculationType || 1,
        })
        setOpen(true)
    }

    return (
        <Card>
            <CardHeader>
                <CardTitle>Locations Management</CardTitle>
                <CardDescription>Manage your business branches and assign specific currencies.</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
                <div className="flex justify-end">
                    <Button onClick={() => setOpen(true)} size="sm">
                        <Plus className="h-4 w-4 mr-2" /> Add Location
                    </Button>
                </div>

                <div className="border rounded-md">
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Name</TableHead>
                                <TableHead>Type</TableHead>
                                <TableHead>Address</TableHead>
                                <TableHead>Currency</TableHead>
                                <TableHead>VAT Mode</TableHead>
                                <TableHead className="text-right">Actions</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {loading ? (
                                <TableRow>
                                    <TableCell colSpan={5} className="h-24 text-center">
                                        <Loader2 className="h-6 w-6 animate-spin mx-auto" />
                                    </TableCell>
                                </TableRow>
                            ) : locations.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={5} className="h-24 text-center text-muted-foreground">
                                        No locations found.
                                    </TableCell>
                                </TableRow>
                            ) : (
                                locations.map((location) => {
                                    // Find currency code if available (assuming we fetched list)
                                    // Though Location DTO might have Currency object if expanded. Default is just ID.
                                    // We can map ID to code from `currencies`.
                                    // Ensure currencies is an array before finding
                                    const curr = Array.isArray(currencies) ? currencies.find(c => c.id === location.currencyId) : null
                                    return (
                                        <TableRow key={location.id}>
                                            <TableCell className="font-medium flex items-center gap-2">
                                                <MapPin className="h-4 w-4 text-muted-foreground" />
                                                {location.locationName}
                                            </TableCell>
                                            <TableCell>{location.locationType}</TableCell>
                                            <TableCell>{location.addressLine1 || "-"}</TableCell>
                                            <TableCell>{curr ? curr.currencyCode : "-"}</TableCell>
                                            <TableCell>
                                                <div className={`inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold ${location.vatCalculationType === 2 ? "bg-blue-100 text-blue-800" : "bg-gray-100 text-gray-800"}`}>
                                                    {location.vatCalculationType === 2 ? "Inclusive" : "Exclusive"}
                                                </div>
                                            </TableCell>
                                            <TableCell className="text-right">
                                                <Button variant="ghost" size="icon" onClick={() => handleEdit(location)}>
                                                    <Pencil className="h-4 w-4" />
                                                </Button>
                                            </TableCell>
                                        </TableRow>
                                    )
                                })
                            )}
                        </TableBody>
                    </Table>
                </div>

                <Dialog open={open} onOpenChange={setOpen}>
                    <DialogContent>
                        <DialogHeader>
                            <DialogTitle>{editingLocation ? "Edit Location" : "Add New Location"}</DialogTitle>
                            <DialogDescription>
                                {editingLocation ? "Update location details." : "Create a new branch or warehouse."}
                            </DialogDescription>
                        </DialogHeader>
                        <Form {...form}>
                            <form onSubmit={form.handleSubmit(onSubmit as any)} className="space-y-4">
                                <FormField
                                    control={form.control}
                                    name="locationName"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Location Name</FormLabel>
                                            <FormControl>
                                                <Input placeholder="Main Branch" {...field} />
                                            </FormControl>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />
                                <FormField
                                    control={form.control}
                                    name="locationType"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Location Type</FormLabel>
                                            <Select onValueChange={field.onChange} defaultValue={field.value}>
                                                <FormControl>
                                                    <SelectTrigger>
                                                        <SelectValue placeholder="Select type" />
                                                    </SelectTrigger>
                                                </FormControl>
                                                <SelectContent>
                                                    <SelectItem value="Store">Store</SelectItem>
                                                    <SelectItem value="Warehouse">Warehouse</SelectItem>
                                                    <SelectItem value="Office">Office</SelectItem>
                                                </SelectContent>
                                            </Select>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />
                                <FormField
                                    control={form.control}
                                    name="currencyId"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Default Currency (Optional)</FormLabel>
                                            <Select onValueChange={field.onChange} defaultValue={field.value} value={field.value}>
                                                <FormControl>
                                                    <SelectTrigger>
                                                        <SelectValue placeholder="Select currency" />
                                                    </SelectTrigger>
                                                </FormControl>
                                                <SelectContent>
                                                    <SelectItem value="none">None</SelectItem>
                                                    {currencies.map(c => (
                                                        <SelectItem key={c.id || c.currencyCode} value={c.id}>
                                                            {c.currencyName} ({c.currencyCode})
                                                        </SelectItem>
                                                    ))}
                                                </SelectContent>
                                            </Select>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />
                                <FormField
                                    control={form.control}
                                    name="addressLine1"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Address</FormLabel>
                                            <FormControl>
                                                <Input placeholder="123 Main St" {...field} />
                                            </FormControl>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />
                                <DialogFooter>
                                    <Button type="submit">{editingLocation ? "Update" : "Create Location"}</Button>
                                </DialogFooter>
                            </form>
                        </Form>
                    </DialogContent>
                </Dialog>
            </CardContent>
        </Card>
    )
}
