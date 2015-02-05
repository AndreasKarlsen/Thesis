package STM.Test; /**
 * Created by Kasper on 03-02-2015.
 */

import org.multiverse.api.references.*;

import java.util.Date;

import static org.multiverse.api.StmUtils.*;

public class Main {

    public static void main(String [] args){
        System.out.print("Hej");
        Account a1 = new Account(1000);
        Account a2 = new Account(500);
        transfer(a1,a2,250);
    }

    public static void transfer(final Account from, final Account to, final int amount){
        atomic(new Runnable(){
            public void run(){
                Date date = new Date();

                from.incBalance(-amount, date);
                to.incBalance(amount, date);
            }
        });
    }
}
