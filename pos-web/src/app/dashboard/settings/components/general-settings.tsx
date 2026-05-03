import { useEffect, useState } from "react"
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useAuth } from "@/hooks/use-auth"
import { settingsService, Currency } from "@/services/settings-service"
import { Button } from "@/components/ui/button"
import { Loader2 } from "lucide-react"

export function GeneralSettings() {
    const { tenant, selectedCompany, selectedLocation, user } = useAuth()
    const [currency, setCurrency] = useState<Currency | null>(null)
    const [currentTime, setCurrentTime] = useState(new Date())
    const [loading, setLoading] = useState(true)

    // Company Editing State
    const [companyData, setCompanyData] = useState<any>(null)
    const [saving, setSaving] = useState(false)

    // Live clock
    useEffect(() => {
        const timer = setInterval(() => setCurrentTime(new Date()), 1000)
        return () => clearInterval(timer)
    }, [])

    // Load Company Data for Editing
    useEffect(() => {
        if (selectedCompany?.companyId) {
            settingsService.getCompany(selectedCompany.companyId)
                .then(data => setCompanyData(data))
                .catch(err => console.error("Failed to load company details", err));
        }
    }, [selectedCompany]);

    // Fetch Currency for selected location
    useEffect(() => {
        async function fetchRegionalData() {
            if (!selectedLocation) {
                setLoading(false)
                return
            }
            try {
                const locations = await settingsService.getLocations()
                const fullLocation = locations.find(l => l.id === selectedLocation.id)

                if (fullLocation?.currencyId) {
                    const currencies = await settingsService.getCurrencies()
                    const currencyList = Array.isArray(currencies) ? currencies : (currencies ? [currencies] : [])
                    const foundCurrency = currencyList.find(c => c.id === fullLocation.currencyId)
                    setCurrency(foundCurrency || null)
                }
            } catch (error) {
                console.error("Failed to fetch regional settings", error)
            } finally {
                setLoading(false)
            }
        }

        fetchRegionalData()
    }, [selectedLocation])

    const handleSaveCompany = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!companyData) return;
        setSaving(true);
        try {
            await settingsService.updateCompany(companyData.companyId, companyData);
            // toast.success("Company profile updated"); // Assuming toast is available or use console
        } catch (error) {
            console.error("Failed to save company", error);
        } finally {
            setSaving(false);
        }
    }

    return (
        <div className="grid gap-6">
            <Card>
                <CardHeader>
                    <CardTitle>Company Profile</CardTitle>
                    <CardDescription>
                        Manage your company's public information.
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    <form onSubmit={handleSaveCompany} className="space-y-4">
                        <div className="grid grid-cols-2 gap-4">
                            <div className="space-y-2">
                                <Label htmlFor="tenant">Tenant Name</Label>
                                <Input id="tenant" value={tenant?.tenantName || ""} disabled />
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="email">Company Email</Label>
                                <Input id="email" value={user?.companyEmailAddress || user?.email || ""} disabled />
                            </div>
                        </div>

                        <div className="space-y-2">
                            <Label htmlFor="companyName">Company Name</Label>
                            <Input
                                id="companyName"
                                value={companyData?.companyName || selectedCompany?.companyName || ""}
                                onChange={(e) => setCompanyData({ ...companyData, companyName: e.target.value })}
                                disabled={!companyData}
                            />
                        </div>
                        <div className="space-y-2">
                            <Label htmlFor="address">Address</Label>
                            <Input
                                id="address"
                                value={companyData?.addressLine1 || ""}
                                onChange={(e) => setCompanyData({ ...companyData, addressLine1: e.target.value })}
                                placeholder="123 Main St..."
                                disabled={!companyData}
                            />
                        </div>
                        <div className="space-y-2">
                            <Label htmlFor="taxId">Tax ID</Label>
                            <Input
                                id="taxId"
                                value={companyData?.taxId || ""}
                                onChange={(e) => setCompanyData({ ...companyData, taxId: e.target.value })}
                                placeholder="TIN-000000"
                                disabled={!companyData}
                            />
                        </div>

                        <div className="pt-2 flex justify-end">
                            <Button type="submit" disabled={saving || !companyData}>
                                {saving && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                                Save Changes
                            </Button>
                        </div>
                    </form>
                </CardContent>
            </Card>

            <Card>
                <CardHeader>
                    <CardTitle>Regional Settings</CardTitle>
                    <CardDescription>
                        Currency and Time based on your current location.
                    </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <Label htmlFor="currency">Default Currency</Label>
                            <div className="relative">
                                <Input
                                    id="currency"
                                    value={loading ? "Loading..." : (currency ? `${currency.currencyName} (${currency.currencyCode})` : "Not Set")}
                                    disabled
                                />
                                {loading && <Loader2 className="absolute right-3 top-2.5 h-4 w-4 animate-spin text-muted-foreground" />}
                            </div>
                        </div>
                        <div className="space-y-2">
                            <Label htmlFor="time">Current Time</Label>
                            <Input
                                id="time"
                                value={currentTime.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' })}
                                disabled
                            />
                        </div>
                    </div>
                </CardContent>
            </Card>
        </div>
    )
}
