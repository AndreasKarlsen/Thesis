/**
 * Created by Kasper on 03-02-2015.
 */

import java.util.Date;

import static org.multiverse.api.StmUtils.atomic;

public class Main {

    public static void main(String [] args){
        Account a1 = new Account(1000);
        Account a2 = new Account(500);
        printAccounts(a1, a2);
        transfer(a1, a2, 250);
        a1.incBalance(100, new Date());
        printAccounts(a1, a2);
    }

    private static void printAccounts(Account... accounts){
            for (Account account : accounts) {
                System.out.println(account);
            }
    }

    public static void transfer(final Account from, final Account to, final int amount){
        atomic(() -> {
            Date date = new Date();

            from.incBalance(-amount, date);
            to.incBalance(amount, date);
        });
    }
}