"use client";

import { useEffect, useState } from "react";
import { Check, ChevronsUpDown, Plus, User } from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import {
    Command,
    CommandEmpty,
    CommandGroup,
    CommandInput,
    CommandItem,
    CommandList,
} from "@/components/ui/command";
import {
    Popover,
    PopoverContent,
    PopoverTrigger,
} from "@/components/ui/popover";
import { Customer } from "@/types/crm";
import { customerService } from "@/services/customer-service";
import { usePosStore } from "@/store/pos-store";

export function CustomerSelector() {
    const [open, setOpen] = useState(false);
    const [customers, setCustomers] = useState<Customer[]>([]);
    const [loading, setLoading] = useState(false);

    // Store
    const selectedCustomer = usePosStore(state => state.selectedCustomer);
    const setCustomer = usePosStore(state => state.setCustomer);

    // Initial load
    useEffect(() => {
        loadCustomers();
    }, []);

    const loadCustomers = async (query?: string) => {
        setLoading(true);
        try {
            const data = await customerService.getCustomers(query);
            setCustomers(data);
        } catch (error) {
            console.error("Failed to load customers", error);
        } finally {
            setLoading(false);
        }
    };

    // For now simple load all. If searching is needed, bind CommandInput logic.
    // Ideally Command component filters locally if list is small. 
    // If backend search is needed, use `onValueChange` and debounce.
    // For now, assume < 100 customers and local filtering by Command.

    return (
        <Popover open={open} onOpenChange={setOpen}>
            <PopoverTrigger asChild>
                <Button
                    variant="outline"
                    role="combobox"
                    aria-expanded={open}
                    className="w-full justify-between"
                >
                    {selectedCustomer ? (
                        <div className="flex items-center gap-2">
                            <User className="h-4 w-4" />
                            <div className="flex flex-col items-start text-xs text-left">
                                <span className="font-semibold">{selectedCustomer.firstName} {selectedCustomer.lastName}</span>
                            </div>
                        </div>
                    ) : (
                        "Select Customer..."
                    )}
                    <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                </Button>
            </PopoverTrigger>
            <PopoverContent className="w-[300px] p-0">
                <Command>
                    <CommandInput placeholder="Search customer..." />
                    <CommandList>
                        <CommandEmpty>No customer found.</CommandEmpty>
                        <CommandGroup>
                            <CommandItem
                                onSelect={() => {
                                    setCustomer(null); // Walk-in / Guest
                                    setOpen(false);
                                }}
                                className="cursor-pointer"
                            >
                                <Check
                                    className={cn(
                                        "mr-2 h-4 w-4",
                                        !selectedCustomer ? "opacity-100" : "opacity-0"
                                    )}
                                />
                                Walk-in Customer (Guest)
                            </CommandItem>

                            {customers.map((customer) => (
                                <CommandItem
                                    key={customer.id}
                                    value={`${customer.firstName} ${customer.lastName} ${customer.phone}`} // search key
                                    onSelect={() => {
                                        setCustomer(customer);
                                        setOpen(false);
                                    }}
                                    className="cursor-pointer"
                                >
                                    <Check
                                        className={cn(
                                            "mr-2 h-4 w-4",
                                            selectedCustomer?.id === customer.id ? "opacity-100" : "opacity-0"
                                        )}
                                    />
                                    <div className="flex flex-col">
                                        <span>{customer.firstName} {customer.lastName}</span>
                                        {customer.companyName && <span className="text-xs text-muted-foreground">{customer.companyName}</span>}
                                    </div>
                                </CommandItem>
                            ))}
                        </CommandGroup>
                    </CommandList>
                </Command>
            </PopoverContent>
        </Popover>
    );
}
