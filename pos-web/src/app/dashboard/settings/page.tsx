"use client"

import { Separator } from "@/components/ui/separator"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { GeneralSettings } from "./components/general-settings"
// import { SecuritySettings } from "./components/security-settings"
// import { NotificationSettings } from "./components/notification-settings"
// import { DisplaySettings } from "./components/display-settings"

import { CurrencySettings } from "./components/currency-settings"
import { LocationSettings } from "./components/location-settings"
import { PaymentMethodSettings } from "./components/payment-method-settings"
import { RoleSettings } from "./components/role-settings"
import { UserSettings } from "./components/user-settings"
import { DeletedUsersSettings } from "./components/deleted-users-settings"
import { VATSettings } from "./components/vat-setup-settings"
import { CountrySettings } from "./components/country-setup-settings"
import { ExpenseTypeSettings } from "./components/expense-type-settings"

export default function SettingsPage() {
    return (
        <div className="space-y-6 max-w-5xl mx-auto pb-10">
            <div>
                <h3 className="text-2xl font-bold tracking-tight">Settings</h3>
                <p className="text-sm text-muted-foreground">
                    Manage your store preferences, locations, and configurations.
                </p>
            </div>
            <Separator />
            <Tabs defaultValue="general" className="space-y-4">
                <TabsList className="h-auto w-full justify-start flex-wrap gap-2 bg-transparent p-0">
                    <TabsTrigger value="general" className="data-[state=active]:bg-primary/10">General</TabsTrigger>
                    <TabsTrigger value="locations" className="data-[state=active]:bg-primary/10">Locations</TabsTrigger>
                    <TabsTrigger value="currency" className="data-[state=active]:bg-primary/10">Currencies</TabsTrigger>
                    <TabsTrigger value="payments" className="data-[state=active]:bg-primary/10">Payments</TabsTrigger>
                    <TabsTrigger value="roles" className="data-[state=active]:bg-primary/10">Roles</TabsTrigger>
                    <TabsTrigger value="users" className="data-[state=active]:bg-primary/10">Users</TabsTrigger>
                    <TabsTrigger value="countries" className="data-[state=active]:bg-primary/10">Country Setup</TabsTrigger>
                    <TabsTrigger value="vat" className="data-[state=active]:bg-primary/10">VAT Setup</TabsTrigger>
                    <TabsTrigger value="expenses" className="data-[state=active]:bg-primary/10">Expense Types</TabsTrigger>
                    <TabsTrigger value="deleted-users" className="data-[state=active]:bg-primary/10">Deleted Users</TabsTrigger>
                    {/* <TabsTrigger value="security" className="data-[state=active]:bg-primary/10">Security</TabsTrigger>
                    <TabsTrigger value="notifications" className="data-[state=active]:bg-primary/10">Notifications</TabsTrigger>
                    <TabsTrigger value="display" className="data-[state=active]:bg-primary/10">Display</TabsTrigger> */}
                </TabsList>

                <TabsContent value="general" className="space-y-4">
                    <GeneralSettings />
                </TabsContent>
                <TabsContent value="locations" className="space-y-4">
                    <LocationSettings />
                </TabsContent>
                <TabsContent value="currency" className="space-y-4">
                    <CurrencySettings />
                </TabsContent>
                <TabsContent value="payments" className="space-y-4">
                    <PaymentMethodSettings />
                </TabsContent>
                <TabsContent value="roles" className="space-y-4">
                    <RoleSettings />
                </TabsContent>
                <TabsContent value="users" className="space-y-4">
                    <UserSettings />
                </TabsContent>
                <TabsContent value="deleted-users" className="space-y-4">
                    <DeletedUsersSettings />
                </TabsContent>
                <TabsContent value="countries" className="space-y-4">
                    <CountrySettings />
                </TabsContent>
                <TabsContent value="vat" className="space-y-4">
                    <VATSettings />
                </TabsContent>
                <TabsContent value="expenses" className="space-y-4">
                    <ExpenseTypeSettings />
                </TabsContent>
                {/* <TabsContent value="security" className="space-y-4">
                    <SecuritySettings />
                </TabsContent>
                <TabsContent value="notifications" className="space-y-4">
                    <NotificationSettings />
                </TabsContent>
                <TabsContent value="display" className="space-y-4">
                    <DisplaySettings />
                </TabsContent> */}
            </Tabs>
        </div>
    )
}
